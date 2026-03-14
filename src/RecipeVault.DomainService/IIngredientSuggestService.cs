using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IIngredientSuggestService {
        Task<List<IngredientSuggestionDto>> SuggestAsync(string query, int limit = 10);
    }
}
