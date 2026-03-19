using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using Cortside.AspNetCore.EntityFramework;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public class MealPlanRepository : IMealPlanRepository {
        private readonly IRecipeVaultDbContext context;

        public MealPlanRepository(IRecipeVaultDbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedList<MealPlan>> SearchAsync(MealPlanSearch model) {
            var plans = model.Build(context.MealPlans
                .Include(x => x.Entries)
                    .ThenInclude(e => e.Recipe)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject));

            var result = new PagedList<MealPlan> {
                PageNumber = model.PageNumber,
                PageSize = model.PageSize,
                TotalItems = await plans.CountAsync().ConfigureAwait(false),
                Items = [],
            };

            plans = plans.ToSortedQuery(model.Sort);
            result.Items = await plans.ToPagedQuery(model.PageNumber, model.PageSize).ToListAsync().ConfigureAwait(false);

            return result;
        }

        public async Task<MealPlan> AddAsync(MealPlan mealPlan) {
            var entity = await context.MealPlans.AddAsync(mealPlan);
            return entity.Entity;
        }

        public Task<MealPlan> GetAsync(Guid id) {
            return context.MealPlans
                .Include(x => x.Entries)
                    .ThenInclude(e => e.Recipe)
                        .ThenInclude(r => r.Ingredients)
                .Include(x => x.CreatedSubject)
                .Include(x => x.LastModifiedSubject)
                .FirstOrDefaultAsync(x => x.MealPlanResourceId == id);
        }

        public Task RemoveAsync(MealPlan mealPlan) {
            context.RemoveRange(mealPlan.Entries);
            context.Remove(mealPlan);
            return Task.CompletedTask;
        }

        public void RemoveEntries(MealPlan mealPlan) {
            context.RemoveRange(mealPlan.Entries);
        }
    }
}
