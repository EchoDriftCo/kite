using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Data.Searches;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    public interface IRecipeService {
        Task<Recipe> CreateRecipeAsync(UpdateRecipeDto dto);
        Task<Recipe> GetRecipeAsync(Guid recipeResourceId);
        Task<PagedList<Recipe>> SearchRecipesAsync(RecipeSearch search);
        Task<Recipe> UpdateRecipeAsync(Guid resourceId, UpdateRecipeDto dto);
        Task DeleteRecipeAsync(Guid resourceId);
        Task<ParseRecipeResponseDto> ParseRecipeImageAsync(ParseRecipeRequestDto request);
        Task SetRecipeVisibilityAsync(Guid recipeResourceId, bool isPublic);
        Task<Recipe> AssignTagsToRecipeAsync(Guid recipeResourceId, List<AssignTagDto> tags);
        Task<Recipe> RemoveTagFromRecipeAsync(Guid recipeResourceId, Guid tagResourceId);
        Task AnalyzeAndApplyDietaryTagsAsync(Recipe recipe);
        Task SetRecipeRatingAsync(Guid recipeResourceId, int? rating);
        Task SetRecipeFavoriteAsync(Guid recipeResourceId, bool isFavorite);
        Task GenerateShareTokenAsync(Guid recipeResourceId);
        Task RevokeShareTokenAsync(Guid recipeResourceId);
        Task<Recipe> GetRecipeByShareTokenAsync(string shareToken);
        Task<Recipe> ForkRecipeAsync(Guid recipeResourceId, string newTitle = null);
        Task<PagedList<Recipe>> GetRecipeForksAsync(Guid recipeResourceId, int pageNumber = 1, int pageSize = 20);
        Task<CookingDataDto> GetCookingDataAsync(Guid recipeResourceId);
    }
}
