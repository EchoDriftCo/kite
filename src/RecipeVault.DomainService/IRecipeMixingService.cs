using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.DomainService.Models;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Service for mixing two recipes together using AI
    /// </summary>
    public interface IRecipeMixingService {
        /// <summary>
        /// Mix two recipes using Gemini AI
        /// </summary>
        /// <param name="recipeA">First recipe to mix</param>
        /// <param name="recipeB">Second recipe to mix</param>
        /// <param name="intent">User's mixing intent (for guided mode)</param>
        /// <param name="mode">Mixing mode: guided, surprise, bestOfBoth</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Mixed recipe preview with attribution notes</returns>
        Task<MixedRecipePreview> MixRecipesAsync(
            Recipe recipeA, 
            Recipe recipeB, 
            string intent, 
            string mode, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Refine a mixed recipe preview based on user feedback
        /// </summary>
        /// <param name="preview">Current recipe preview</param>
        /// <param name="refinementNotes">User's refinement notes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refined recipe preview</returns>
        Task<MixedRecipePreview> RefineMixedRecipeAsync(
            MixedRecipePreview preview,
            string refinementNotes,
            CancellationToken cancellationToken = default);
    }
}
