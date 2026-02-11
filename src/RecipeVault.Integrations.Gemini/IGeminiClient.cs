using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RecipeVault.Integrations.Gemini {
    /// <summary>
    /// Client for interacting with Google Gemini API for recipe parsing
    /// </summary>
    public interface IGeminiClient {
        /// <summary>
        /// Parse a recipe image using Gemini API
        /// </summary>
        /// <param name="imageBase64">Base64 encoded image data</param>
        /// <param name="mimeType">MIME type of the image (e.g., image/jpeg, image/png)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed recipe response from Gemini</returns>
        /// <exception cref="GeminiApiException">Thrown when API returns error or no recipe detected</exception>
        Task<GeminiParseResponse> ParseRecipeAsync(string imageBase64, string mimeType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consolidate a grocery list by intelligently merging similar ingredients
        /// </summary>
        /// <param name="items">Raw grocery items to consolidate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Consolidated grocery list response</returns>
        Task<GeminiGroceryConsolidationResponse> ConsolidateGroceryListAsync(List<GeminiGroceryItem> items, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Input grocery item for consolidation
    /// </summary>
    public class GeminiGroceryItem {
        public string Item { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
    }

    /// <summary>
    /// Response from Gemini grocery list consolidation
    /// </summary>
    public class GeminiGroceryConsolidationResponse {
        public List<GeminiConsolidatedItem> Items { get; set; } = new();
    }

    /// <summary>
    /// A consolidated grocery item
    /// </summary>
    public class GeminiConsolidatedItem {
        public string Item { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
    }

    /// <summary>
    /// Response from Gemini API containing parsed recipe data
    /// </summary>
    public class GeminiParseResponse {
        /// <summary>
        /// Confidence score (0-1) indicating parse quality
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Parsed recipe title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Recipe yield (number of servings)
        /// </summary>
        public int? Yield { get; set; }

        /// <summary>
        /// Preparation time in minutes
        /// </summary>
        public int? PrepTimeMinutes { get; set; }

        /// <summary>
        /// Cook time in minutes
        /// </summary>
        public int? CookTimeMinutes { get; set; }

        /// <summary>
        /// List of ingredients with quantities and units
        /// </summary>
        public List<GeminiIngredient> Ingredients { get; set; }

        /// <summary>
        /// Numbered instructions for recipe preparation
        /// </summary>
        public List<GeminiInstruction> Instructions { get; set; }

        /// <summary>
        /// Warnings for fields that were uncertain or missing
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Parsed ingredient with quantity and preparation notes
    /// </summary>
    public class GeminiIngredient {
        /// <summary>
        /// Quantity of ingredient (decimal for proper fractions like 0.5 for 1/2)
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Unit of measurement (normalized: cup, tbsp, tsp, oz, lb, g, kg, ml, l, piece, pinch, dash)
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Name of the ingredient (e.g., "all-purpose flour")
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Preparation notes (e.g., "sifted", "room temperature")
        /// </summary>
        public string Preparation { get; set; }

        /// <summary>
        /// Original text from image for reference
        /// </summary>
        public string RawText { get; set; }
    }

    /// <summary>
    /// Single instruction step from recipe
    /// </summary>
    public class GeminiInstruction {
        /// <summary>
        /// Step number in sequence
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// Cleaned up instruction text
        /// </summary>
        public string Instruction { get; set; }

        /// <summary>
        /// Original text from image for reference
        /// </summary>
        public string RawText { get; set; }
    }
}
