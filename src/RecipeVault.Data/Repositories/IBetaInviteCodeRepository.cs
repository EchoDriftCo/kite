using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IBetaInviteCodeRepository {
        Task<BetaInviteCode> AddAsync(BetaInviteCode betaInviteCode);
        Task<BetaInviteCode> GetAsync(Guid betaInviteCodeResourceId);
        Task<BetaInviteCode> GetByCodeAsync(string code);
        Task<PagedList<BetaInviteCode>> SearchAsync(BetaInviteCodeSearch search);
    }
}
