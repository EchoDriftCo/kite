using System;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IRecipeRepository {
        Task<Recipe> AddAsync(Recipe recipe);
        Task<Recipe> GetAsync(Guid id);
        Task<Recipe> GetByIdAsync(int recipeId);
        Task<PagedList<Recipe>> SearchAsync(RecipeSearch model);
        Task RemoveAsync(Recipe recipe);
    }
}
