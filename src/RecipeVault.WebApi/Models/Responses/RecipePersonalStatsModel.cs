#pragma warning disable CS1591 // Missing XML comments

using System;

namespace RecipeVault.WebApi.Models.Responses {
    public class RecipePersonalStatsModel {
        public int CookCount { get; set; }
        public double? AverageRating { get; set; }
        public DateTime LastCookedDate { get; set; }
    }
}
