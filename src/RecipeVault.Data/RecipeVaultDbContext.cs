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

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.SetDateTime();
            modelBuilder.SetCascadeDelete();
        }
    }
}
