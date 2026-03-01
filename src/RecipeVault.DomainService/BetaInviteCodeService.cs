using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Exceptions;

namespace RecipeVault.DomainService {
    public class BetaInviteCodeService : IBetaInviteCodeService {
        private readonly ILogger<BetaInviteCodeService> logger;
        private readonly IBetaInviteCodeRepository betaInviteCodeRepository;
        private readonly ISubjectPrincipal subjectPrincipal;

        public BetaInviteCodeService(IBetaInviteCodeRepository betaInviteCodeRepository, ILogger<BetaInviteCodeService> logger, ISubjectPrincipal subjectPrincipal) {
            this.logger = logger;
            this.betaInviteCodeRepository = betaInviteCodeRepository;
            this.subjectPrincipal = subjectPrincipal;
        }

        public async Task<BetaInviteCode> CreateCodeAsync(CreateBetaInviteCodeDto dto) {
            var entity = new BetaInviteCode(dto.Code, dto.MaxUses, dto.ExpiresDate);
            await betaInviteCodeRepository.AddAsync(entity).ConfigureAwait(false);
            logger.LogInformation("Created new beta invite code {Code}", dto.Code);
            return entity;
        }

        public Task<PagedList<BetaInviteCode>> SearchCodesAsync(BetaInviteCodeSearch search) {
            return betaInviteCodeRepository.SearchAsync(search);
        }

        public async Task<ValidateInviteCodeResultDto> ValidateCodeAsync(string code) {
            var entity = await betaInviteCodeRepository.GetByCodeAsync(code).ConfigureAwait(false);

            if (entity == null) {
                return new ValidateInviteCodeResultDto { IsValid = false, Message = "Invalid invite code" };
            }

            if (!entity.IsActive) {
                return new ValidateInviteCodeResultDto { IsValid = false, Message = "Invite code is no longer active" };
            }

            if (entity.IsExpired()) {
                return new ValidateInviteCodeResultDto { IsValid = false, Message = "Invite code has expired" };
            }

            if (!entity.HasUsesRemaining()) {
                return new ValidateInviteCodeResultDto { IsValid = false, Message = "Invite code has reached its maximum number of uses" };
            }

            return new ValidateInviteCodeResultDto { IsValid = true, Message = "Invite code is valid" };
        }

        public async Task<BetaInviteCode> RedeemCodeAsync(string code) {
            var entity = await betaInviteCodeRepository.GetByCodeAsync(code).ConfigureAwait(false);

            if (entity == null) {
                throw new InvalidInviteCodeException("Invalid invite code");
            }

            if (!entity.IsActive) {
                throw new InvalidInviteCodeException("Invite code is no longer active");
            }

            if (entity.IsExpired()) {
                throw new InviteCodeExpiredException("Invite code has expired");
            }

            if (!entity.HasUsesRemaining()) {
                throw new InviteCodeMaxUsesReachedException("Invite code has reached its maximum number of uses");
            }

            var currentSubjectId = Guid.Parse(subjectPrincipal.SubjectId);
            var redemption = new BetaInviteCodeRedemption(entity.BetaInviteCodeId, currentSubjectId);
            entity.AddRedemption(redemption);
            entity.IncrementUseCount();

            logger.LogInformation("Redeemed beta invite code {Code} for subject {SubjectId}", code, currentSubjectId);
            return entity;
        }
    }
}
