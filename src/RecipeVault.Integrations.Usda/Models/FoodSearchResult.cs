using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RecipeVault.Integrations.Usda.Models {
    public class FoodSearchResponse {
        [JsonPropertyName("foods")]
        public List<FoodSearchResult>? Foods { get; set; }

        [JsonPropertyName("totalHits")]
        public int TotalHits { get; set; }
    }

    public class FoodSearchResult {
        [JsonPropertyName("fdcId")]
        public int FdcId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("dataType")]
        public string? DataType { get; set; }

        [JsonPropertyName("brandOwner")]
        public string? BrandOwner { get; set; }

        [JsonPropertyName("foodNutrients")]
        public List<FoodNutrient>? FoodNutrients { get; set; }
    }

    public class FoodNutrient {
        [JsonPropertyName("nutrientId")]
        public int NutrientId { get; set; }

        [JsonPropertyName("nutrientName")]
        public string? NutrientName { get; set; }

        [JsonPropertyName("nutrientNumber")]
        public string? NutrientNumber { get; set; }

        [JsonPropertyName("unitName")]
        public string? UnitName { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }
}
