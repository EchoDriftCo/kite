using System.Collections.Generic;

namespace RecipeVault.Dto.Output {
    public class CookingStatsDto {
        public int TotalCooks { get; set; }
        public int UniqueRecipes { get; set; }
        public int CooksThisYear { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public List<MostCookedRecipeDto> MostCookedRecipes { get; set; }
    }
}
