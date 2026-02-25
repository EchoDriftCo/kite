using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data {
    public interface IRecipeVaultDbContext {
        DbSet<Recipe> Recipes { get; set; }
        DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        DbSet<Unit> Units { get; set; }
        DbSet<UnitAlias> UnitAliases { get; set; }
        DbSet<MealPlan> MealPlans { get; set; }
        DbSet<MealPlanEntry> MealPlanEntries { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<RecipeTag> RecipeTags { get; set; }
        DbSet<Circle> Circles { get; set; }
        DbSet<CircleMember> CircleMembers { get; set; }
        DbSet<CircleRecipe> CircleRecipes { get; set; }
        DbSet<CircleInvite> CircleInvites { get; set; }
        DbSet<IngredientNutrition> IngredientNutritions { get; set; }
        DbSet<RecipeNutrition> RecipeNutritions { get; set; }
        DbSet<ImportJob> ImportJobs { get; set; }

        void RemoveRange(IEnumerable<object> entities);
        EntityEntry Remove(object entity);
        int SaveChanges();
        System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
    }
}
