using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade {
    public interface IBetaInviteCodeFacade {
        Task<BetaInviteCodeDto> CreateCodeAsync(CreateBetaInviteCodeDto dto);
        Task<PagedList<BetaInviteCodeDto>> SearchCodesAsync(BetaInviteCodeSearchDto search);
        Task<ValidateInviteCodeResultDto> ValidateCodeAsync(string code);
        Task<BetaInviteCodeDto> RedeemCodeAsync(RedeemInviteCodeDto dto);
    }
}
