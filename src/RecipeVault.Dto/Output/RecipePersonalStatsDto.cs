using System;

namespace RecipeVault.Dto.Output {
    public class RecipePersonalStatsDto {
        public int CookCount { get; set; }
        public double? AverageRating { get; set; }
        public DateTime LastCookedDate { get; set; }
        public string LastNote { get; set; }
    }
}
