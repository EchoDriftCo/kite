using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IBetaInviteCodeService {
        Task<List<BetaInviteCode>> CreateCodesAsync(CreateBetaInviteCodeDto dto);
        Task<PagedList<BetaInviteCode>> SearchCodesAsync(BetaInviteCodeSearch search);
        Task<ValidateInviteCodeResultDto> ValidateCodeAsync(string code);
        Task<RedeemCodeResultDto> RedeemCodeAsync(string code, Guid subjectId);
        Task<BetaInviteCode> DeactivateCodeAsync(string code);
    }
}
