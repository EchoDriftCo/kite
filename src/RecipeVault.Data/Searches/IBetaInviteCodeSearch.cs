using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public interface IBetaInviteCodeSearch : ISearch, ISearchBuilder<BetaInviteCode> {
        bool? IsActive { get; set; }
    }
}
