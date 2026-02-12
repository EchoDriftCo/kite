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
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitAlias> UnitAliases { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<MealPlanEntry> MealPlanEntries { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<RecipeTag> RecipeTags { get; set; }

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
        }
    }
}
