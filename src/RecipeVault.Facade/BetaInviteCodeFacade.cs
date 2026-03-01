using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
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

        public BetaInviteCodeFacade(ILogger<BetaInviteCodeFacade> logger, IUnitOfWork uow, IBetaInviteCodeService betaInviteCodeService, BetaInviteCodeMapper mapper) {
            this.uow = uow;
            this.betaInviteCodeService = betaInviteCodeService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<BetaInviteCodeDto> CreateCodeAsync(CreateBetaInviteCodeDto dto) {
            var entity = await betaInviteCodeService.CreateCodeAsync(dto).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(entity);
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

        public async Task<BetaInviteCodeDto> RedeemCodeAsync(RedeemInviteCodeDto dto) {
            var entity = await betaInviteCodeService.RedeemCodeAsync(dto.Code).ConfigureAwait(false);
            await uow.SaveChangesAsync().ConfigureAwait(false);
            return mapper.MapToDto(entity);
        }
    }
}
