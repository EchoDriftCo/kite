using Cortside.AspNetCore.Auditable;
using Cortside.AspNetCore.Auditable.Entities;
using Cortside.AspNetCore.Common;
using Cortside.AspNetCore.EntityFramework;
using Cortside.Common.Security;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data {
    public class RecipeVaultDbContext : UnitOfWorkContext<Subject>, IRecipeVaultDbContext {
        public RecipeVaultDbContext(DbContextOptions options, ISubjectPrincipal subjectPrincipal, ISubjectFactory<Subject> subjectFactory) : base(options, subjectPrincipal, subjectFactory) {
            DateTimeHandling = InternalDateTimeHandling.Utc;
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitAlias> UnitAliases { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<MealPlanEntry> MealPlanEntries { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<RecipeTag> RecipeTags { get; set; }
        public DbSet<Circle> Circles { get; set; }
        public DbSet<CircleMember> CircleMembers { get; set; }
        public DbSet<CircleRecipe> CircleRecipes { get; set; }
        public DbSet<CircleInvite> CircleInvites { get; set; }
        public DbSet<IngredientNutrition> IngredientNutritions { get; set; }
        public DbSet<RecipeNutrition> RecipeNutritions { get; set; }
        public DbSet<ImportJob> ImportJobs { get; set; }
        public DbSet<DietaryProfile> DietaryProfiles { get; set; }
        public DbSet<DietaryRestriction> DietaryRestrictions { get; set; }
        public DbSet<AvoidedIngredient> AvoidedIngredients { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionRecipe> CollectionRecipes { get; set; }
        public DbSet<CookingLog> CookingLogs { get; set; }
        public DbSet<CookingLogPhoto> CookingLogPhotos { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<UserEquipment> UserEquipment { get; set; }
        public DbSet<RecipeEquipment> RecipeEquipment { get; set; }
        public DbSet<RecipeLink> RecipeLinks { get; set; }
        public DbSet<BetaInviteCode> BetaInviteCodes { get; set; }
        public DbSet<BetaInviteCodeRedemption> BetaInviteCodeRedemptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.SetDateTime();
            modelBuilder.SetCascadeDelete();

            // Override restrict → cascade for entities owned by a Recipe
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne<Recipe>()
                .WithMany(r => r.Ingredients)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeInstruction>()
                .HasOne<Recipe>()
                .WithMany(r => r.Instructions)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeTag>()
                .HasOne(rt => rt.Recipe)
                .WithMany(r => r.RecipeTags)
                .HasForeignKey(rt => rt.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MealPlanEntry>()
                .HasOne(me => me.Recipe)
                .WithMany()
                .HasForeignKey(me => me.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MealPlanEntry>()
                .HasOne(me => me.MealPlan)
                .WithMany(mp => mp.Entries)
                .HasForeignKey(me => me.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Circle cascade deletes
            modelBuilder.Entity<CircleMember>()
                .HasOne(cm => cm.Circle)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.CircleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CircleRecipe>()
                .HasOne(cr => cr.Circle)
                .WithMany(c => c.SharedRecipes)
                .HasForeignKey(cr => cr.CircleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CircleInvite>()
                .HasOne(ci => ci.Circle)
                .WithMany(c => c.Invites)
                .HasForeignKey(ci => ci.CircleId)
                .OnDelete(DeleteBehavior.Cascade);

            // CircleRecipe → Recipe (restrict delete if recipe is shared in circles)
            modelBuilder.Entity<CircleRecipe>()
                .HasOne(cr => cr.Recipe)
                .WithMany()
                .HasForeignKey(cr => cr.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nutrition cascade deletes
            modelBuilder.Entity<IngredientNutrition>()
                .HasOne(i_n => i_n.RecipeIngredient)
                .WithMany()
                .HasForeignKey(i_n => i_n.RecipeIngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeNutrition>()
                .HasOne(rn => rn.Recipe)
                .WithMany()
                .HasForeignKey(rn => rn.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Dietary profiles cascade deletes
            modelBuilder.Entity<DietaryRestriction>()
                .HasOne(dr => dr.DietaryProfile)
                .WithMany(dp => dp.Restrictions)
                .HasForeignKey(dr => dr.DietaryProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AvoidedIngredient>()
                .HasOne(ai => ai.DietaryProfile)
                .WithMany(dp => dp.AvoidedIngredients)
                .HasForeignKey(ai => ai.DietaryProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Collection cascade deletes
            modelBuilder.Entity<CollectionRecipe>()
                .HasOne(cr => cr.Collection)
                .WithMany(c => c.CollectionRecipes)
                .HasForeignKey(cr => cr.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CollectionRecipe>()
                .HasOne(cr => cr.Recipe)
                .WithMany()
                .HasForeignKey(cr => cr.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cooking log cascade deletes
            modelBuilder.Entity<CookingLog>()
                .HasOne(cl => cl.Recipe)
                .WithMany()
                .HasForeignKey(cl => cl.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CookingLogPhoto>()
                .HasOne(clp => clp.CookingLog)
                .WithMany(cl => cl.Photos)
                .HasForeignKey(clp => clp.CookingLogId)
                .OnDelete(DeleteBehavior.Cascade);

            // Equipment cascade deletes
            modelBuilder.Entity<RecipeEquipment>()
                .HasOne(re => re.Recipe)
                .WithMany(r => r.RecipeEquipment)
                .HasForeignKey(re => re.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeEquipment>()
                .HasOne(re => re.Equipment)
                .WithMany()
                .HasForeignKey(re => re.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserEquipment>()
                .HasOne(ue => ue.Equipment)
                .WithMany()
                .HasForeignKey(ue => ue.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recipe links cascade deletes
            modelBuilder.Entity<RecipeLink>()
                .HasOne(rl => rl.ParentRecipe)
                .WithMany(r => r.LinkedRecipes)
                .HasForeignKey(rl => rl.ParentRecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeLink>()
                .HasOne(rl => rl.LinkedRecipe)
                .WithMany(r => r.UsedInRecipes)
                .HasForeignKey(rl => rl.LinkedRecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Beta invite code cascade deletes
            modelBuilder.Entity<BetaInviteCodeRedemption>()
                .HasOne(r => r.BetaInviteCode)
                .WithMany(c => c.Redemptions)
                .HasForeignKey(r => r.BetaInviteCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Recipe mixing relationships (AI Fusion)
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.MixedFromRecipeA)
                .WithMany()
                .HasForeignKey(r => r.MixedFromRecipeAId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.MixedFromRecipeB)
                .WithMany()
                .HasForeignKey(r => r.MixedFromRecipeBId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
