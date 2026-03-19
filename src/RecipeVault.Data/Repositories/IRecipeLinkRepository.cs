using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IRecipeLinkRepository {
        Task<RecipeLink> AddAsync(RecipeLink recipeLink);
        Task<RecipeLink> GetAsync(Guid recipeLinkResourceId);
        Task<RecipeLink> GetByIdAsync(int recipeLinkId);
        Task<List<RecipeLink>> GetLinkedRecipesAsync(int parentRecipeId);
        Task<List<RecipeLink>> GetUsedInRecipesAsync(int linkedRecipeId);
        Task RemoveAsync(RecipeLink recipeLink);
        Task<bool> HasCircularLinkAsync(int parentRecipeId, int linkedRecipeId);
    }
}
