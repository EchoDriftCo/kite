using System.Linq;
using Cortside.AspNetCore.EntityFramework.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Searches {
    public class BetaInviteCodeSearch : Search, IBetaInviteCodeSearch {
        public bool? IsActive { get; set; }

        public IQueryable<BetaInviteCode> Build(IQueryable<BetaInviteCode> entities) {
            if (IsActive.HasValue) {
                entities = entities.Where(x => x.IsActive == IsActive.Value);
            }

            return entities;
        }
    }
}
