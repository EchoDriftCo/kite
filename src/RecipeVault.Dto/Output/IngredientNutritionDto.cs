using System;

namespace RecipeVault.Dto.Output {
    public class IngredientNutritionDto {
        public int IngredientNutritionId { get; set; }
        public int RecipeIngredientId { get; set; }
        public int? FdcId { get; set; }
        public string MatchedFoodName { get; set; }
        public decimal MatchConfidence { get; set; }
        
        // Nutrition values
        public decimal? Calories { get; set; }
        public decimal? ProteinGrams { get; set; }
        public decimal? CarbsGrams { get; set; }
        public decimal? FatGrams { get; set; }
        public decimal? FiberGrams { get; set; }
        public decimal? SugarGrams { get; set; }
        public decimal? SodiumMg { get; set; }
        
        public decimal GramsUsed { get; set; }
        public DateTime CalculatedDate { get; set; }
        public bool IsManualOverride { get; set; }
    }
}
