using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    /// <summary>
    /// Response DTO containing substitution suggestions
    /// </summary>
    public class SubstitutionResponseDto {
        /// <summary>
        /// Optional analysis summary
        /// </summary>
        public string Analysis { get; set; }

        /// <summary>
        /// Substitution options for each ingredient
        /// </summary>
        public List<IngredientSubstitutionDto> Substitutions { get; set; } = new();

        /// <summary>
        /// Whether this response came from cache
        /// </summary>
        public bool Cached { get; set; }
    }

    /// <summary>
    /// Substitution options for a single ingredient
    /// </summary>
    public class IngredientSubstitutionDto {
        /// <summary>
        /// Index of the original ingredient
        /// </summary>
        public int OriginalIndex { get; set; }

        /// <summary>
        /// Original ingredient text
        /// </summary>
        public string OriginalText { get; set; }

        /// <summary>
        /// Reason this ingredient needs substitution (dietary mode only)
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Available substitution options
        /// </summary>
        public List<SubstitutionOptionDto> Options { get; set; } = new();
    }

    /// <summary>
    /// A single substitution option
    /// </summary>
    public class SubstitutionOptionDto {
        /// <summary>
        /// Descriptive name for this option
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Replacement ingredients
        /// </summary>
        public List<SubstitutionIngredientDto> Ingredients { get; set; } = new();

        /// <summary>
        /// Notes on taste/texture impact
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Optional technique adjustments needed
        /// </summary>
        public string TechniqueAdjustments { get; set; }
    }

    /// <summary>
    /// An ingredient in a substitution option
    /// </summary>
    public class SubstitutionIngredientDto {
        /// <summary>
        /// Quantity as decimal
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Unit of measurement
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Ingredient item name
        /// </summary>
        public string Item { get; set; }
    }
}
