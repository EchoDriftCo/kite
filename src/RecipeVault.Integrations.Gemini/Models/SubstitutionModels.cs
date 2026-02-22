using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RecipeVault.Integrations.Gemini.Models {
    /// <summary>
    /// Substitution analysis request to Gemini
    /// </summary>
    public class SubstitutionAnalysisRequest {
        public string RecipeTitle { get; set; }
        public List<string> AllIngredients { get; set; }
        public string InstructionsSummary { get; set; }
        public List<int> TargetIngredientIndices { get; set; }
        public List<string> DietaryConstraints { get; set; }
    }

    /// <summary>
    /// Response from Gemini substitution analysis
    /// </summary>
    public class GeminiSubstitutionAnalysis {
        [JsonPropertyName("analysis")]
        public string Analysis { get; set; }

        [JsonPropertyName("substitutions")]
        public List<GeminiIngredientSubstitution> Substitutions { get; set; } = new();
    }

    /// <summary>
    /// Substitution options for a single ingredient
    /// </summary>
    public class GeminiIngredientSubstitution {
        [JsonPropertyName("originalIndex")]
        public int OriginalIndex { get; set; }

        [JsonPropertyName("original")]
        public string Original { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("options")]
        public List<GeminiSubstitutionOption> Options { get; set; } = new();
    }

    /// <summary>
    /// A single substitution option for an ingredient
    /// </summary>
    public class GeminiSubstitutionOption {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ingredients")]
        public List<GeminiSubstitutionIngredient> Ingredients { get; set; } = new();

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("techniqueAdjustments")]
        public string TechniqueAdjustments { get; set; }
    }

    /// <summary>
    /// A single ingredient in a substitution option
    /// </summary>
    public class GeminiSubstitutionIngredient {
        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("item")]
        public string Item { get; set; }
    }
}
