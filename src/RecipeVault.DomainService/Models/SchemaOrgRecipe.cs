using System.Text.Json.Serialization;

namespace RecipeVault.DomainService.Models {
    /// <summary>
    /// Schema.org Recipe format (JSON-LD)
    /// https://schema.org/Recipe
    /// </summary>
    public class SchemaOrgRecipe {
        [JsonPropertyName("@type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("recipeYield")]
        [JsonConverter(typeof(FlexibleYieldConverter))]
        public int? RecipeYield { get; set; }

        [JsonPropertyName("prepTime")]
        public string PrepTime { get; set; }

        [JsonPropertyName("cookTime")]
        public string CookTime { get; set; }

        [JsonPropertyName("totalTime")]
        public string TotalTime { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("recipeIngredient")]
        public string[] RecipeIngredient { get; set; }

        [JsonPropertyName("recipeInstructions")]
        [JsonConverter(typeof(FlexibleInstructionsConverter))]
        public string[] RecipeInstructions { get; set; }

        [JsonPropertyName("image")]
        [JsonConverter(typeof(FlexibleImageConverter))]
        public string Image { get; set; }

        [JsonPropertyName("author")]
        [JsonConverter(typeof(FlexibleAuthorConverter))]
        public string Author { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
