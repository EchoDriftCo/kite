using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Integrations.Usda.Models;

namespace RecipeVault.Integrations.Usda {
    public interface IUsdaFoodDataService {
        Task<FoodSearchResponse> SearchFoodsAsync(string query, int pageSize = 10);
        Task<FoodDetails> GetFoodDetailsAsync(int fdcId);
        Task<List<FoodSearchResult>> SearchWithBestMatchAsync(string ingredientText, int maxResults = 5);
    }
}
