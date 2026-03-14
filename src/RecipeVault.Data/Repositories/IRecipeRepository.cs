using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;

namespace RecipeVault.Data.Repositories {
    public interface IRecipeRepository {
        Task<Recipe> AddAsync(Recipe recipe);
        Task<Recipe> GetAsync(Guid id);
        Task<Recipe> GetByIdAsync(int recipeId);
        Task<List<Recipe>> GetByIdsAsync(List<int> recipeIds);
        Task<PagedList<Recipe>> SearchAsync(RecipeSearch model);
        Task RemoveAsync(Recipe recipe);
        Task<Recipe> GetByShareTokenAsync(string shareToken);
        Task<Recipe> GetBySourceAsync(Guid subjectId, string source);
    }
}
