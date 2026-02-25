using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IDietaryConflictService {
        Task<DietaryConflictCheckDto> CheckRecipeAsync(Recipe recipe, DietaryProfile profile);
        Task<DietaryConflictCheckDto> CheckRecipeByIdAsync(int recipeId, int profileId);
    }
}
