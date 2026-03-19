using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RecipeVault.Integrations.Usda.Models {
    public class FoodDetails {
        [JsonPropertyName("fdcId")]
        public int FdcId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        [JsonPropertyName("foodNutrients")]
        public List<DetailedFoodNutrient> FoodNutrients { get; set; }

        [JsonPropertyName("foodPortions")]
        public List<FoodPortion> FoodPortions { get; set; }
    }

    public class DetailedFoodNutrient {
        [JsonPropertyName("nutrient")]
        public Nutrient Nutrient { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class Nutrient {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("unitName")]
        public string UnitName { get; set; }
    }

    public class FoodPortion {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("measureUnit")]
        public MeasureUnit MeasureUnit { get; set; }

        [JsonPropertyName("modifier")]
        public string Modifier { get; set; }

        [JsonPropertyName("gramWeight")]
        public decimal GramWeight { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class MeasureUnit {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
