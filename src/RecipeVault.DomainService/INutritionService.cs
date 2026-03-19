using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.DomainService {
    public interface INutritionService {
        Task<RecipeNutrition> AnalyzeRecipeNutritionAsync(int recipeId);
        Task<RecipeNutrition> GetRecipeNutritionAsync(int recipeId);
        Task<IngredientNutrition> UpdateIngredientNutritionAsync(int recipeIngredientId, int? fdcId, string matchedFoodName, decimal matchConfidence);
        Task<FoodSearchResponse> SearchFoodsAsync(string query);
    }
}
