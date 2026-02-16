using System;
using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    /// <summary>
    /// Response DTO from recipe parsing operation
    /// </summary>
    public class ParseRecipeResponseDto {
        /// <summary>
        /// Confidence score (0-1) indicating parse quality
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Parsed recipe data
        /// </summary>
        public ParsedRecipeDto Parsed { get; set; }

        /// <summary>
        /// List of warnings about uncertain or missing fields
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Parsed recipe data from AI
    /// </summary>
    public class ParsedRecipeDto {
        /// <summary>
        /// Recipe title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Recipe yield (number of servings)
        /// </summary>
        public int? Yield { get; set; }

        /// <summary>
        /// Prep time in minutes
        /// </summary>
        public int? PrepTimeMinutes { get; set; }

        /// <summary>
        /// Cook time in minutes
        /// </summary>
        public int? CookTimeMinutes { get; set; }

        /// <summary>
        /// List of ingredients
        /// </summary>
        public List<ParsedIngredientDto> Ingredients { get; set; } = new();

        /// <summary>
        /// List of preparation instructions
        /// </summary>
        public List<ParsedInstructionDto> Instructions { get; set; } = new();

        /// <summary>
        /// Image URL extracted from source webpage (og:image)
        /// </summary>
        public string ImageUrl { get; set; }
    }

    /// <summary>
    /// Parsed ingredient with quantity and unit
    /// </summary>
    public class ParsedIngredientDto {
        /// <summary>
        /// Quantity as decimal (e.g., 0.5 for 1/2)
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Unit of measurement (normalized)
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Ingredient name
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Preparation notes (e.g., "sifted", "room temperature")
        /// </summary>
        public string Preparation { get; set; }

        /// <summary>
        /// Original text from image
        /// </summary>
        public string RawText { get; set; }
    }

    /// <summary>
    /// Single instruction step
    /// </summary>
    public class ParsedInstructionDto {
        /// <summary>
        /// Step number in sequence
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// Cleaned up instruction text
        /// </summary>
        public string Instruction { get; set; }

        /// <summary>
        /// Original text from image
        /// </summary>
        public string RawText { get; set; }
    }
}
