using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    /// <summary>
    /// DTO for a generated recipe preview (before saving)
    /// </summary>
    public class GeneratedRecipeDto {
        /// <summary>
        /// Generated recipe title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Generated recipe description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Number of servings
        /// </summary>
        public int Yield { get; set; }

        /// <summary>
        /// Prep time in minutes
        /// </summary>
        public int? PrepTimeMinutes { get; set; }

        /// <summary>
        /// Cook time in minutes
        /// </summary>
        public int? CookTimeMinutes { get; set; }

        /// <summary>
        /// Total time in minutes (prep + cook)
        /// </summary>
        public int? TotalTimeMinutes => PrepTimeMinutes.HasValue && CookTimeMinutes.HasValue
            ? PrepTimeMinutes.Value + CookTimeMinutes.Value
            : null;

        /// <summary>
        /// List of ingredients with quantities
        /// </summary>
        public List<GeneratedIngredientDto> Ingredients { get; set; } = new();

        /// <summary>
        /// Step-by-step instructions
        /// </summary>
        public List<GeneratedInstructionDto> Instructions { get; set; } = new();

        /// <summary>
        /// Suggested tags for the recipe
        /// </summary>
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// DTO for a generated ingredient
    /// </summary>
    public class GeneratedIngredientDto {
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public string Item { get; set; }
        public string Preparation { get; set; }
    }

    /// <summary>
    /// DTO for a generated instruction
    /// </summary>
    public class GeneratedInstructionDto {
        public int StepNumber { get; set; }
        public string Instruction { get; set; }
    }
}
