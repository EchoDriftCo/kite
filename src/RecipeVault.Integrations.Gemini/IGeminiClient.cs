using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RecipeVault.Integrations.Gemini.Models;

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
        /// Parse recipe text (e.g., from a webpage) using Gemini API
        /// </summary>
        /// <param name="recipeText">Text content containing the recipe</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed recipe response from Gemini</returns>
        /// <exception cref="GeminiApiException">Thrown when API returns error or no recipe detected</exception>
        Task<GeminiParseResponse> ParseRecipeTextAsync(string recipeText, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consolidate a grocery list by intelligently merging similar ingredients
        /// </summary>
        /// <param name="items">Raw grocery items to consolidate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Consolidated grocery list response</returns>
        Task<GeminiGroceryConsolidationResponse> ConsolidateGroceryListAsync(List<GeminiGroceryItem> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyze recipe ingredients to infer dietary tags
        /// </summary>
        /// <param name="ingredients">List of ingredient text strings to analyze</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dietary tags inferred from the ingredient list</returns>
        Task<GeminiDietaryAnalysisResponse> AnalyzeDietaryTagsAsync(List<string> ingredients, CancellationToken cancellationToken = default);

        /// <summary>
        /// Normalize an entity name (Chef, Restaurant, or Cookbook) to check if it's a known entity
        /// </summary>
        /// <param name="entityName">The entity name to normalize</param>
        /// <param name="sourceType">The type of source (Chef, Restaurant, or Cookbook)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Entity normalization result, or null if not a known entity</returns>
        Task<GeminiEntityNormalizationResponse> NormalizeEntityAsync(string entityName, int sourceType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyze recipe and suggest ingredient substitutions
        /// </summary>
        /// <param name="recipeTitle">Recipe title</param>
        /// <param name="allIngredients">Full list of recipe ingredients</param>
        /// <param name="instructionsSummary">Condensed instructions for context</param>
        /// <param name="targetIngredientIndices">Indices of specific ingredients to substitute (optional)</param>
        /// <param name="dietaryConstraints">Dietary constraints to apply (e.g., "Gluten-Free")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Substitution analysis with options for each ingredient</returns>
        Task<GeminiSubstitutionAnalysis> AnalyzeSubstitutionsAsync(
            string recipeTitle,
            List<string> allIngredients,
            string instructionsSummary,
            List<int> targetIngredientIndices,
            List<string> dietaryConstraints,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a text prompt to Gemini and get raw text response (for recipe generation, etc.)
        /// </summary>
        /// <param name="prompt">Text prompt to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Raw text response from Gemini</returns>
        Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate text using Gemini AI with a custom prompt and response MIME type
        /// </summary>
        /// <param name="prompt">The prompt to send to Gemini</param>
        /// <param name="responseMimeType">Expected response MIME type (default: application/json)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated text response from Gemini</returns>
        Task<string> GenerateTextAsync(string prompt, string responseMimeType, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Response from Gemini entity normalization
    /// </summary>
    public class GeminiEntityNormalizationResponse {
        /// <summary>
        /// Whether the entity was recognized as a known entity
        /// </summary>
        public bool IsRecognized { get; set; }

        /// <summary>
        /// Normalized entity ID (slug form, e.g., "bobby-flay")
        /// </summary>
        public string NormalizedEntityId { get; set; }

        /// <summary>
        /// Canonical name of the entity (e.g., "Bobby Flay")
        /// </summary>
        public string CanonicalName { get; set; }

        /// <summary>
        /// Confidence score (0-1) for the recognition
        /// </summary>
        public decimal Confidence { get; set; }
    }

    /// <summary>
    /// Response from Gemini dietary tag analysis
    /// </summary>
    public class GeminiDietaryAnalysisResponse {
        public List<GeminiDietaryTag> Tags { get; set; } = new();
    }

    /// <summary>
    /// A single dietary tag inferred by Gemini
    /// </summary>
    public class GeminiDietaryTag {
        public string Name { get; set; }
        public decimal Confidence { get; set; }
    }

    /// <summary>
    /// Input grocery item for consolidation
    /// </summary>
    public class GeminiGroceryItem {
        public string Item { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public List<string> Sources { get; set; } = new();
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
        public string Category { get; set; }
        public List<string> Sources { get; set; } = new();
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
