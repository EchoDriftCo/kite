using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IIngredientSearchFacade {
        Task<PagedList<IngredientSearchResultDto>> SearchByIngredientsAsync(IngredientSearchRequestDto request);
        Task<List<IngredientSuggestionDto>> SuggestAsync(string query, int limit = 10);
    }
}
