using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade {
    public interface IMealPlanFacade {
        Task<MealPlanDto> CreateMealPlanAsync(UpdateMealPlanDto dto);
        Task<MealPlanDto> GetMealPlanAsync(Guid resourceId);
        Task<PagedList<MealPlanDto>> SearchMealPlansAsync(MealPlanSearchDto search);
        Task<MealPlanDto> UpdateMealPlanAsync(Guid resourceId, UpdateMealPlanDto dto);
        Task DeleteMealPlanAsync(Guid resourceId);
        Task<GroceryListDto> GetGroceryListAsync(Guid resourceId);
    }
}
