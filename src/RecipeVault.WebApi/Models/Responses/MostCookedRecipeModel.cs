#pragma warning disable CS1591 // Missing XML comments

using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class MostCookedRecipeModel {
        public Guid RecipeResourceId { get; set; }
        public string RecipeTitle { get; set; }
        public int CookCount { get; set; }
        public double? AverageRating { get; set; }
        public DateTime LastCookedDate { get; set; }
    }
}
