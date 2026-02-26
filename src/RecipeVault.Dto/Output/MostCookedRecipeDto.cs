using System;

namespace RecipeVault.Dto.Output {
    public class MostCookedRecipeDto {
        public int RecipeId { get; set; }
        public Guid RecipeResourceId { get; set; }
        public string RecipeTitle { get; set; }
        public int CookCount { get; set; }
        public double? AverageRating { get; set; }
        public DateTime LastCookedDate { get; set; }
    }
}
