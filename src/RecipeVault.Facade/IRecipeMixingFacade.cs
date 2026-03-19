using System;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    /// <summary>
    /// Facade for recipe mixing operations
    /// </summary>
    public interface IRecipeMixingFacade {
        /// <summary>
        /// Mix two recipes together using AI
        /// </summary>
        Task<MixedRecipePreviewDto> MixRecipesAsync(MixRecipesRequestDto request);

        /// <summary>
        /// Refine a mixed recipe preview
        /// </summary>
        Task<MixedRecipePreviewDto> RefineMixedRecipeAsync(RefineMixRequestDto request);

        /// <summary>
        /// Save a mixed recipe preview as a real recipe
        /// </summary>
        Task<RecipeDto> SaveMixedRecipeAsync(MixedRecipePreviewDto preview);
    }
}
