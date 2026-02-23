using System.Collections.Generic;
using RecipeVault.Dto.Output;

namespace RecipeVault.Dto.Input {
    /// <summary>
    /// Request DTO for applying substitutions and creating a fork
    /// </summary>
    public class ApplySubstitutionsDto {
        /// <summary>
        /// Selected substitution for each ingredient
        /// </summary>
        public List<SubstitutionSelectionDto> Selections { get; set; } = new();

        /// <summary>
        /// Optional custom title for the forked recipe
        /// </summary>
        public string ForkTitle { get; set; }
    }

    /// <summary>
    /// A single substitution selection
    /// </summary>
    public class SubstitutionSelectionDto {
        /// <summary>
        /// Index of the ingredient being substituted
        /// </summary>
        public int IngredientIndex { get; set; }

        /// <summary>
        /// Index of the selected substitution option (preferred - more efficient)
        /// </summary>
        public int? OptionIndex { get; set; }

        /// <summary>
        /// The selected substitution option data (alternative to OptionIndex)
        /// </summary>
        public SubstitutionOptionDto SelectedOption { get; set; }
    }
}
