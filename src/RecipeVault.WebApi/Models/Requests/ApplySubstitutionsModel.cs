using System.Collections.Generic;
using RecipeVault.WebApi.Models.Responses;

namespace RecipeVault.WebApi.Models.Requests {
    /// <summary>
    /// Request model for applying substitutions and creating a fork
    /// </summary>
    public class ApplySubstitutionsModel {
        /// <summary>
        /// Selected substitution for each ingredient
        /// </summary>
        public List<SubstitutionSelectionModel> Selections { get; set; } = new();

        /// <summary>
        /// Optional custom title for the forked recipe
        /// </summary>
        public string ForkTitle { get; set; }
    }

    /// <summary>
    /// A single substitution selection
    /// </summary>
    public class SubstitutionSelectionModel {
        /// <summary>
        /// Index of the ingredient being substituted
        /// </summary>
        public int IngredientIndex { get; set; }

        /// <summary>
        /// The selected substitution option data
        /// </summary>
        public SubstitutionOptionModel SelectedOption { get; set; }
    }
}
