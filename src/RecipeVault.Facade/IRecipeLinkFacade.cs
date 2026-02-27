using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    public interface IRecipeLinkFacade {
        Task<RecipeLinkDto> CreateRecipeLinkAsync(Guid parentRecipeResourceId, CreateRecipeLinkDto dto);
        Task<RecipeLinkDto> UpdateRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId, UpdateRecipeLinkDto dto);
        Task DeleteRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId);
        Task<List<LinkedRecipeDto>> GetLinkedRecipesAsync(Guid parentRecipeResourceId);
        Task<List<UsedInRecipeDto>> GetUsedInRecipesAsync(Guid recipeResourceId);
        Task<List<RecipeDto>> SearchLinkableRecipesAsync(string query);
    }
}
