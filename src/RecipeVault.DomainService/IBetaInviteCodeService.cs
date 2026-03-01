using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IBetaInviteCodeService {
        Task<BetaInviteCode> CreateCodeAsync(CreateBetaInviteCodeDto dto);
        Task<PagedList<BetaInviteCode>> SearchCodesAsync(BetaInviteCodeSearch search);
        Task<ValidateInviteCodeResultDto> ValidateCodeAsync(string code);
        Task<BetaInviteCode> RedeemCodeAsync(string code);
    }
}
