#pragma warning disable CS1591 // Missing XML comments

namespace RecipeVault.WebApi.Models.Responses {
    public class OnboardingStatusModel {
        public bool HasCompletedOnboarding { get; set; }
        public int RecipeCount { get; set; }
        public bool HasDietaryProfile { get; set; }
        public bool HasImportedRecipes { get; set; }
        public OnboardingProgressModel Progress { get; set; }
    }
}
