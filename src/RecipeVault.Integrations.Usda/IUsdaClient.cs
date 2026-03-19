using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public interface IUsdaClient {
        /// <summary>
        /// Search for foods by query string
        /// </summary>
        Task<FoodSearchResponse> SearchFoodsAsync(string query, int pageSize = 10);

        /// <summary>
        /// Get detailed food information including full nutrient data
        /// </summary>
        Task<FoodDetails> GetFoodDetailsAsync(int fdcId);

        /// <summary>
        /// Check if API is configured and available
        /// </summary>
        bool IsAvailable();
    }
}
