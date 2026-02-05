using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data {
    public interface IRecipeVaultDbContext {
        DbSet<Recipe> Recipes { get; set; }
        DbSet<Unit> Units { get; set; }
        DbSet<UnitAlias> UnitAliases { get; set; }

        void RemoveRange(IEnumerable<object> entities);
        EntityEntry Remove(object entity);
    }
}
