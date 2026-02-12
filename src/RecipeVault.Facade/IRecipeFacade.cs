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
    }
}
