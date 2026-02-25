using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cortside.AspNetCore.Common.Paging;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;
using RecipeVault.Dto.Search;

namespace RecipeVault.Facade {
    public interface IRecipeFacade {
        Task<RecipeDto> CreateRecipeAsync(UpdateRecipeDto dto);
        Task<RecipeDto> GetRecipeAsync(Guid resourceId);
        Task<PagedList<RecipeDto>> SearchRecipesAsync(RecipeSearchDto search);
        Task<RecipeDto> UpdateRecipeAsync(Guid resourceId, UpdateRecipeDto dto);
        Task DeleteRecipeAsync(Guid resourceId);
        Task<ParseRecipeResponseDto> ParseRecipeImageAsync(ParseRecipeRequestDto request);
        Task<RecipeDto> SetRecipeVisibilityAsync(Guid resourceId, bool isPublic);
        Task<RecipeDto> AssignTagsAsync(Guid recipeResourceId, List<AssignTagDto> tags);
        Task<RecipeDto> RemoveTagAsync(Guid recipeResourceId, Guid tagResourceId);
        Task<RecipeDto> AnalyzeDietaryTagsAsync(Guid recipeResourceId);
        Task<RecipeDto> SetRecipeRatingAsync(Guid resourceId, int? rating);
        Task<RecipeDto> SetRecipeFavoriteAsync(Guid resourceId, bool isFavorite);
        Task<RecipeDto> GenerateShareTokenAsync(Guid resourceId);
        Task<RecipeDto> RevokeShareTokenAsync(Guid resourceId);
        Task<RecipeDto> GetRecipeByShareTokenAsync(string shareToken);
        Task<RecipeDto> ForkRecipeAsync(Guid recipeResourceId, string newTitle = null);
        Task<PagedList<RecipeDto>> GetRecipeForksAsync(Guid recipeResourceId, int pageNumber = 1, int pageSize = 20);
        Task<SubstitutionResponseDto> GetSubstitutionsAsync(Guid recipeResourceId, SubstitutionRequestDto request);
        Task<RecipeDto> ApplySubstitutionsAsync(Guid recipeResourceId, ApplySubstitutionsDto request);
        Task<CookingDataDto> GetCookingDataAsync(Guid recipeResourceId);
    }
}
