using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IMealPlanService {
        Task<MealPlan> CreateMealPlanAsync(UpdateMealPlanDto dto);
        Task<MealPlan> GetMealPlanAsync(Guid mealPlanResourceId);
        Task<PagedList<MealPlan>> SearchMealPlansAsync(MealPlanSearch search);
        Task<MealPlan> UpdateMealPlanAsync(Guid resourceId, UpdateMealPlanDto dto);
        Task DeleteMealPlanAsync(Guid resourceId);
        Task<GroceryListDto> GenerateGroceryListAsync(Guid mealPlanResourceId);
    }
}
