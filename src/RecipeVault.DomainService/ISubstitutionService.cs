using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RecipeVault.Domain.Entities;
using RecipeVault.Dto.Input;
using RecipeVault.Dto.Output;

namespace RecipeVault.DomainService {
    /// <summary>
    /// Service for handling ingredient substitution suggestions and application
    /// </summary>
    public interface ISubstitutionService {
        /// <summary>
        /// Get substitution suggestions for a recipe
        /// </summary>
        /// <param name="recipeResourceId">Recipe to analyze</param>
        /// <param name="ingredientIndices">Indices of specific ingredients to substitute (optional)</param>
        /// <param name="dietaryConstraints">Dietary constraints to apply (optional)</param>
        /// <returns>Substitution suggestions with caching info</returns>
        Task<SubstitutionResponseDto> GetSubstitutionsAsync(
            Guid recipeResourceId,
            List<int> ingredientIndices,
            List<string> dietaryConstraints);

        /// <summary>
        /// Apply selected substitutions and create a forked recipe
        /// </summary>
        /// <param name="recipeResourceId">Original recipe</param>
        /// <param name="selections">Selected substitution options</param>
        /// <param name="forkTitle">Optional custom title for the fork</param>
        /// <returns>The forked recipe with substitutions applied</returns>
        Task<Recipe> ApplySubstitutionsAsync(
            Guid recipeResourceId,
            List<SubstitutionSelectionDto> selections,
            string forkTitle = null);
    }
}
