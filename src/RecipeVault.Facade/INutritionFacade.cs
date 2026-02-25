using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface INutritionFacade {
        /// <summary>
        /// Calculate and save nutrition for a recipe
        /// </summary>
        Task<RecipeNutritionDto> CalculateRecipeNutritionAsync(Guid recipeResourceId);

        /// <summary>
        /// Get nutrition data for a recipe
        /// </summary>
        Task<RecipeNutritionDto> GetRecipeNutritionAsync(Guid recipeResourceId);

        /// <summary>
        /// Search USDA foods for an ingredient
        /// </summary>
        Task<List<FoodSearchDto>> SearchFoodsAsync(string query);

        /// <summary>
        /// Check if nutrition service is available
        /// </summary>
        bool IsAvailable();
    }
}
