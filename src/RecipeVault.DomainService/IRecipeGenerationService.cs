using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Service for AI-powered recipe generation
    /// </summary>
    public interface IRecipeGenerationService {
        /// <summary>
        /// Generate new recipes based on user prompt and constraints
        /// </summary>
        /// <param name="request">Generation request with prompt and constraints</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated recipe(s) for preview</returns>
        Task<List<GeneratedRecipeDto>> GenerateRecipesAsync(GenerateRecipeRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refine a previously generated recipe
        /// </summary>
        /// <param name="request">Refinement request with previous recipe and refinement instructions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refined recipe</returns>
        Task<GeneratedRecipeDto> RefineRecipeAsync(RefineRecipeRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user has remaining generation quota
        /// </summary>
        /// <returns>Remaining generations for today</returns>
        Task<int> GetRemainingGenerationsAsync();
    }
}
