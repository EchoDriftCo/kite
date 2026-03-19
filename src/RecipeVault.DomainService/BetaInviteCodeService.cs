using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Microsoft.Extensions.Logging;
using RecipeVault.Data.Repositories;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Domain.Enums;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public class BetaInviteCodeService : IBetaInviteCodeService {
        private readonly ILogger<BetaInviteCodeService> logger;
        private readonly IBetaInviteCodeRepository betaInviteCodeRepository;
        private readonly IUserAccountService userAccountService;
        private static readonly Random random = new Random();
        private const string CodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude confusing chars: I, O, 0, 1

        public BetaInviteCodeService(
            IBetaInviteCodeRepository betaInviteCodeRepository,
            IUserAccountService userAccountService,
            ILogger<BetaInviteCodeService> logger) {
            this.logger = logger;
            this.betaInviteCodeRepository = betaInviteCodeRepository;
            this.userAccountService = userAccountService;
        }

        public async Task<List<BetaInviteCode>> CreateCodesAsync(CreateBetaInviteCodeDto dto) {
            var codes = new List<BetaInviteCode>();

            for (int i = 0; i < dto.Count; i++) {
                var code = await GenerateUniqueCodeAsync().ConfigureAwait(false);
                var entity = new BetaInviteCode(code, dto.MaxUses, dto.ExpiresDate);
                await betaInviteCodeRepository.AddAsync(entity).ConfigureAwait(false);
                codes.Add(entity);
            }

            logger.LogInformation("Generated {Count} beta invite codes", dto.Count);
            return codes;
        }

        public Task<PagedList<BetaInviteCode>> SearchCodesAsync(BetaInviteCodeSearch search) {
            return betaInviteCodeRepository.SearchAsync(search);
        }

        public async Task<ValidateInviteCodeResultDto> ValidateCodeAsync(string code) {
            code = code?.ToUpperInvariant();
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

        public async Task<RedeemCodeResultDto> RedeemCodeAsync(string code, Guid subjectId) {
            using (logger.BeginScope("RedeemCode")) {
                logger.LogInformation("Attempting to redeem code {Code} for subject {SubjectId}", code, subjectId);

                code = code?.ToUpperInvariant();
                var entity = await betaInviteCodeRepository.GetByCodeAsync(code).ConfigureAwait(false);

                if (entity == null) {
                    logger.LogWarning("Code {Code} not found", code);
                    return new RedeemCodeResultDto {
                        Success = false,
                        ErrorMessage = "Invalid invite code"
                    };
                }

                if (!entity.IsActive) {
                    logger.LogWarning("Code {Code} is not active", code);
                    return new RedeemCodeResultDto {
                        Success = false,
                        ErrorMessage = "Invite code is no longer active"
                    };
                }

                if (entity.IsExpired()) {
                    logger.LogWarning("Code {Code} is expired", code);
                    return new RedeemCodeResultDto {
                        Success = false,
                        ErrorMessage = "Invite code has expired"
                    };
                }

                if (!entity.HasUsesRemaining()) {
                    logger.LogWarning("Code {Code} has no uses remaining", code);
                    return new RedeemCodeResultDto {
                        Success = false,
                        ErrorMessage = "Invite code has reached its maximum number of uses"
                    };
                }

                // Check if user already redeemed this code
                var existingRedemption = entity.Redemptions?.FirstOrDefault(r => r.SubjectId == subjectId);
                if (existingRedemption != null) {
                    logger.LogWarning("Subject {SubjectId} already redeemed code {Code}", subjectId, code);
                    return new RedeemCodeResultDto {
                        Success = false,
                        ErrorMessage = "You have already redeemed this invite code"
                    };
                }

                // Get or create user account
                var account = await userAccountService.GetOrCreateAccountAsync(subjectId).ConfigureAwait(false);
                var previousTier = account.AccountTier;

                // Upgrade to Beta (only if not already Beta)
                if (previousTier != AccountTier.Beta) {
                    account.SetTier(AccountTier.Beta);
                }

                // Create redemption record
                var redemption = new BetaInviteCodeRedemption(
                    entity.BetaInviteCodeId,
                    subjectId,
                    previousTier,
                    AccountTier.Beta
                );
                entity.AddRedemption(redemption);
                entity.IncrementUseCount();

                logger.LogInformation("Successfully redeemed code {Code} for subject {SubjectId}, upgraded from {PreviousTier} to Beta",
                    code, subjectId, previousTier);

                return new RedeemCodeResultDto {
                    Success = true
                };
            }
        }

        public async Task<BetaInviteCode> DeactivateCodeAsync(string code) {
            code = code?.ToUpperInvariant();
            var entity = await betaInviteCodeRepository.GetByCodeAsync(code).ConfigureAwait(false);

            if (entity == null) {
                logger.LogWarning("Attempted to deactivate non-existent code {Code}", code);
                return null;
            }

            entity.Deactivate();
            logger.LogInformation("Deactivated invite code {Code}", code);
            return entity;
        }

        private async Task<string> GenerateUniqueCodeAsync() {
            string code;
            BetaInviteCode existing;

            do {
                code = GenerateRandomCode();
                existing = await betaInviteCodeRepository.GetByCodeAsync(code).ConfigureAwait(false);
            } while (existing != null);

            return code;
        }

        private static string GenerateRandomCode() {
            // Generate a 14-character code in format: XXXX-XXXX-XXXX
            var parts = new string[3];
            for (int i = 0; i < 3; i++) {
                var chars = new char[4];
                for (int j = 0; j < 4; j++) {
                    chars[j] = CodeCharacters[random.Next(CodeCharacters.Length)];
                }
                parts[i] = new string(chars);
            }
            return string.Join("-", parts);
        }
    }
}
