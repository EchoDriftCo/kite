using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;

namespace RecipeVault.DomainService {
    public interface IRecipeService {
        Task<Recipe> CreateRecipeAsync(UpdateRecipeDto dto);
        Task<Recipe> GetRecipeAsync(Guid recipeResourceId);
        Task<PagedList<Recipe>> SearchRecipesAsync(RecipeSearch search);
        Task<Recipe> UpdateRecipeAsync(Guid resourceId, UpdateRecipeDto dto);
        Task DeleteRecipeAsync(Guid resourceId);
    }
}
