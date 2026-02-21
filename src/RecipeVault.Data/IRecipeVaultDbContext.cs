using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data {
    public interface IRecipeVaultDbContext {
        DbSet<Recipe> Recipes { get; set; }
        DbSet<Unit> Units { get; set; }
        DbSet<UnitAlias> UnitAliases { get; set; }
        DbSet<MealPlan> MealPlans { get; set; }
        DbSet<MealPlanEntry> MealPlanEntries { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<RecipeTag> RecipeTags { get; set; }

        void RemoveRange(IEnumerable<object> entities);
        EntityEntry Remove(object entity);
    }
}
