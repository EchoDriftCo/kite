using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade {
    public interface IBetaInviteCodeFacade {
        Task<List<BetaInviteCodeDto>> CreateCodesAsync(CreateBetaInviteCodeDto dto);
        Task<PagedList<BetaInviteCodeDto>> SearchCodesAsync(BetaInviteCodeSearchDto search);
        Task<ValidateInviteCodeResultDto> ValidateCodeAsync(string code);
        Task<RedeemCodeResultDto> RedeemCodeAsync(RedeemInviteCodeDto dto);
        Task<BetaInviteCodeDto> DeactivateCodeAsync(string code);
    }
}
