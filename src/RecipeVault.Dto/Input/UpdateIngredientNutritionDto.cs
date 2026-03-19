namespace RecipeVault.Dto.Input {
    public class UpdateIngredientNutritionDto {
        public int? FdcId { get; set; }
        public string MatchedFoodName { get; set; }
        public decimal MatchConfidence { get; set; }
    }
}
