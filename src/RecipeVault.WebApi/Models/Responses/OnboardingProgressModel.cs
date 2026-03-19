#pragma warning disable CS1591 // Missing XML comments

namespace RecipeVault.WebApi.Models.Responses {
    public class OnboardingProgressModel {
        public bool DietaryProfileSet { get; set; }
        public bool SamplesAdded { get; set; }
        public bool TourCompleted { get; set; }
    }
}
