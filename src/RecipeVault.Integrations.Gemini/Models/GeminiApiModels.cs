using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RecipeVault.Integrations.Gemini.Models {
    /// <summary>
    /// Request model for Gemini generateContent API
    /// </summary>
    public class GeminiGenerateContentRequest {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; }

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig GenerationConfig { get; set; }
    }

    /// <summary>
    /// Content block containing text and image parts
    /// </summary>
    public class GeminiContent {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; }
    }

    /// <summary>
    /// Part of content (can be text or image)
    /// </summary>
    public class GeminiPart {
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Text { get; set; }

        [JsonPropertyName("inline_data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public GeminiInlineData InlineData { get; set; }
    }

    /// <summary>
    /// Inline image data for API request
    /// </summary>
    public class GeminiInlineData {
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    /// <summary>
    /// Generation configuration for API response format
    /// </summary>
    public class GeminiGenerationConfig {
        [JsonPropertyName("response_mime_type")]
        public string ResponseMimeType { get; set; } = "application/json";
    }

    /// <summary>
    /// Response from Gemini generateContent API
    /// </summary>
    public class GeminiGenerateContentResponse {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate> Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsageMetadata UsageMetadata { get; set; }

        [JsonPropertyName("error")]
        public GeminiError Error { get; set; }
    }

    /// <summary>
    /// Candidate response from Gemini
    /// </summary>
    public class GeminiCandidate {
        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("safetyRatings")]
        public List<GeminiSafetyRating> SafetyRatings { get; set; }
    }

    /// <summary>
    /// Safety rating from Gemini
    /// </summary>
    public class GeminiSafetyRating {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("probability")]
        public string Probability { get; set; }
    }

    /// <summary>
    /// Usage metadata from Gemini response
    /// </summary>
    public class GeminiUsageMetadata {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }

    /// <summary>
    /// Error response from Gemini API
    /// </summary>
    public class GeminiError {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public List<object> Details { get; set; }
    }

    /// <summary>
    /// Expected recipe parsing result from Gemini model
    /// </summary>
    public class GeminiRecipeParseResult {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("yield")]
        public int? Yield { get; set; }

        [JsonPropertyName("prepTimeMinutes")]
        public int? PrepTimeMinutes { get; set; }

        [JsonPropertyName("cookTimeMinutes")]
        public int? CookTimeMinutes { get; set; }

        [JsonPropertyName("ingredients")]
        public List<GeminiRecipeIngredient> Ingredients { get; set; }

        [JsonPropertyName("instructions")]
        public List<GeminiRecipeInstruction> Instructions { get; set; }
    }

    /// <summary>
    /// Ingredient from Gemini recipe parse
    /// </summary>
    public class GeminiRecipeIngredient {
        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("item")]
        public string Item { get; set; }

        [JsonPropertyName("preparation")]
        public string Preparation { get; set; }

        [JsonPropertyName("rawText")]
        public string RawText { get; set; }
    }

    /// <summary>
    /// Instruction from Gemini recipe parse
    /// </summary>
    public class GeminiRecipeInstruction {
        [JsonPropertyName("stepNumber")]
        public int StepNumber { get; set; }

        [JsonPropertyName("instruction")]
        public string Instruction { get; set; }

        [JsonPropertyName("rawText")]
        public string RawText { get; set; }
    }

    /// <summary>
    /// Expected grocery consolidation result from Gemini model
    /// </summary>
    public class GeminiGroceryConsolidationResult {
        [JsonPropertyName("items")]
        public List<GeminiGroceryConsolidatedItem> Items { get; set; }
    }

    /// <summary>
    /// Single consolidated grocery item from Gemini
    /// </summary>
    public class GeminiGroceryConsolidatedItem {
        [JsonPropertyName("item")]
        public string Item { get; set; }

        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }
}
