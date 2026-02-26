#pragma warning disable CS1591 // Missing XML comments

using System.Collections.Generic;

namespace RecipeVault.WebApi.Models.Responses {
    public class CookingStatsModel {
        public int TotalCooks { get; set; }
        public int UniqueRecipes { get; set; }
        public int CooksThisYear { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public List<MostCookedRecipeModel> MostCookedRecipes { get; set; }
    }
}
