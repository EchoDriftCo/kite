using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;

namespace RecipeVault.DomainService {
    public interface IRecipeLinkService {
        Task<RecipeLink> CreateRecipeLinkAsync(Guid parentRecipeResourceId, CreateRecipeLinkDto dto);
        Task<RecipeLink> UpdateRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId, UpdateRecipeLinkDto dto);
        Task DeleteRecipeLinkAsync(Guid parentRecipeResourceId, Guid linkResourceId);
        Task<List<RecipeLink>> GetLinkedRecipesAsync(Guid parentRecipeResourceId);
        Task<List<RecipeLink>> GetUsedInRecipesAsync(Guid recipeResourceId);
        Task<List<Recipe>> SearchLinkableRecipesAsync(string query);
    }
}
