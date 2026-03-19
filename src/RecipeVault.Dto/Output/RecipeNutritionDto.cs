using System;

namespace RecipeVault.Dto.Output {
    public class RecipeNutritionDto {
        public int RecipeNutritionId { get; set; }
        public int RecipeId { get; set; }
        
        // Per-serving values
        public decimal? CaloriesPerServing { get; set; }
        public decimal? ProteinPerServing { get; set; }
        public decimal? CarbsPerServing { get; set; }
        public decimal? FatPerServing { get; set; }
        public decimal? FiberPerServing { get; set; }
        public decimal? SugarPerServing { get; set; }
        public decimal? SodiumPerServing { get; set; }
        
        // Coverage metrics
        public int IngredientsMatched { get; set; }
        public int IngredientsTotal { get; set; }
        public decimal CoveragePercent { get; set; }
        
        public DateTime CalculatedDate { get; set; }
        public bool IsStale { get; set; }
    }
}
