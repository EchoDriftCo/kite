using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IIngredientSearchService {
        Task<List<IngredientSearchResultDto>> SearchByIngredientsAsync(IngredientSearchRequestDto request);
    }
}
