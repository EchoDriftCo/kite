using System.Text.Json.Serialization;

namespace RecipeVault.DomainService.Models {
    public class PaprikaRecipe {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ingredients")]
        public string Ingredients { get; set; }

        [JsonPropertyName("directions")]
        public string Directions { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("source_url")]
        public string SourceUrl { get; set; }

        [JsonPropertyName("servings")]
        public string Servings { get; set; }

        [JsonPropertyName("prep_time")]
        public string PrepTime { get; set; }

        [JsonPropertyName("cook_time")]
        public string CookTime { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("photo_data")]
        public string PhotoData { get; set; }

        [JsonPropertyName("categories")]
        public string[] Categories { get; set; }
    }
}
