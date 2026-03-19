using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.Facade {
    /// <summary>
    /// Facade for AI-powered recipe generation
    /// </summary>
    public interface IRecipeGenerationFacade {
        /// <summary>
        /// Generate new recipes based on user prompt and constraints
        /// </summary>
        Task<List<GeneratedRecipeDto>> GenerateRecipesAsync(GenerateRecipeRequestDto request);

        /// <summary>
        /// Refine a previously generated recipe
        /// </summary>
        Task<GeneratedRecipeDto> RefineRecipeAsync(RefineRecipeRequestDto request);

        /// <summary>
        /// Save a generated recipe to the user's library
        /// </summary>
        /// <param name="generatedRecipe">The generated recipe to save</param>
        /// <returns>Saved recipe DTO</returns>
        Task<RecipeDto> SaveGeneratedRecipeAsync(GeneratedRecipeDto generatedRecipe);

        /// <summary>
        /// Get remaining generation quota for today
        /// </summary>
        Task<int> GetRemainingGenerationsAsync();
    }
}
