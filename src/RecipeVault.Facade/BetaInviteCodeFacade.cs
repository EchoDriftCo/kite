using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.Extensions.Logging;
using RecipeVault.DomainService;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;
using RecipeVault.Facade.Mappers;

namespace RecipeVault.Facade {
    public class BetaInviteCodeFacade : IBetaInviteCodeFacade {
        private readonly IUnitOfWork uow;
        private readonly IBetaInviteCodeService betaInviteCodeService;
        private readonly BetaInviteCodeMapper mapper;
        private readonly ILogger<BetaInviteCodeFacade> logger;
        private readonly ISubjectPrincipal subjectPrincipal;

        public BetaInviteCodeFacade(
            ILogger<BetaInviteCodeFacade> logger,
            IUnitOfWork uow,
            IBetaInviteCodeService betaInviteCodeService,
            BetaInviteCodeMapper mapper,
            ISubjectPrincipal subjectPrincipal) {
            this.uow = uow;
            this.betaInviteCodeService = betaInviteCodeService;
            this.mapper = mapper;
            this.logger = logger;
            this.subjectPrincipal = subjectPrincipal;
        }

        private Guid CurrentSubjectId => Guid.Parse(subjectPrincipal.SubjectId);

        public async Task<List<BetaInviteCodeDto>> CreateCodesAsync(CreateBetaInviteCodeDto dto) {
            var entities = await betaInviteCodeService.CreateCodesAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return entities.Select(x => mapper.MapToDto(x)).ToList();
        }

        public async Task<PagedList<BetaInviteCodeDto>> SearchCodesAsync(BetaInviteCodeSearchDto search) {
            var betaSearch = mapper.Map(search);
            await using (var tx = await uow.BeginReadUncommitedAsync().ConfigureAwait(false)) {
                var results = await betaInviteCodeService.SearchCodesAsync(betaSearch).ConfigureAwait(false);
                return results.Convert(x => mapper.MapToDto(x));
            }
        }

        public async Task<ValidateInviteCodeResultDto> ValidateCodeAsync(string code) {
            await using (var tx = uow.BeginNoTracking()) {
                return await betaInviteCodeService.ValidateCodeAsync(code).ConfigureAwait(false);
            }
        }

        public async Task<RedeemCodeResultDto> RedeemCodeAsync(RedeemInviteCodeDto dto) {
            var result = await betaInviteCodeService.RedeemCodeAsync(dto.Code, CurrentSubjectId).ConfigureAwait(false);
            if (result.Success) {
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
            return result;
        }

        public async Task<BetaInviteCodeDto> DeactivateCodeAsync(string code) {
            var entity = await betaInviteCodeService.DeactivateCodeAsync(code).ConfigureAwait(false);
            if (entity == null) {
                return null;
            }

            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(entity);
        }
    }
}
