using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IMealPlanRepository {
        Task<MealPlan> AddAsync(MealPlan mealPlan);
        Task<MealPlan> GetAsync(Guid id);
        Task<PagedList<MealPlan>> SearchAsync(MealPlanSearch model);
        Task RemoveAsync(MealPlan mealPlan);
        void RemoveEntries(MealPlan mealPlan);
    }
}
