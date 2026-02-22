using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Request model for getting substitution suggestions
    /// </summary>
    public class SubstitutionRequestModel {
        /// <summary>
        /// Indices of specific ingredients to substitute (0-based)
        /// </summary>
        public List<int> IngredientIndices { get; set; } = new();

        /// <summary>
        /// Dietary constraints to apply (e.g., "Gluten-Free", "Vegan")
        /// </summary>
        public List<string> DietaryConstraints { get; set; } = new();
    }
}
