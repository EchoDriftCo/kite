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
        DbSet<DietaryProfile> DietaryProfiles { get; set; }
        DbSet<DietaryRestriction> DietaryRestrictions { get; set; }
        DbSet<AvoidedIngredient> AvoidedIngredients { get; set; }
        DbSet<Collection> Collections { get; set; }
        DbSet<CollectionRecipe> CollectionRecipes { get; set; }
        DbSet<CookingLog> CookingLogs { get; set; }
        DbSet<CookingLogPhoto> CookingLogPhotos { get; set; }
        DbSet<Equipment> Equipment { get; set; }
        DbSet<UserEquipment> UserEquipment { get; set; }
        DbSet<RecipeEquipment> RecipeEquipment { get; set; }
        DbSet<RecipeLink> RecipeLinks { get; set; }
        DbSet<BetaInviteCode> BetaInviteCodes { get; set; }
        DbSet<BetaInviteCodeRedemption> BetaInviteCodeRedemptions { get; set; }
        DbSet<UserAccount> UserAccounts { get; set; }
        DbSet<ApiToken> ApiTokens { get; set; }
        DbSet<IngredientSynonym> IngredientSynonyms { get; set; }
        DbSet<UserPantryItem> UserPantryItems { get; set; }

        void RemoveRange(IEnumerable<object> entities);
        EntityEntry Remove(object entity);
        int SaveChanges();
        System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
    }
}
