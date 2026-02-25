using System;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface INutritionFacade {
        Task<RecipeNutritionDto> AnalyzeRecipeNutritionAsync(Guid recipeResourceId);
        Task<RecipeNutritionDto> GetRecipeNutritionAsync(Guid recipeResourceId);
        Task<IngredientNutritionDto> UpdateIngredientNutritionAsync(Guid recipeResourceId, int ingredientIndex, UpdateIngredientNutritionDto dto);
        Task<FoodSearchDto[]> SearchFoodsAsync(string query);
    }
}
