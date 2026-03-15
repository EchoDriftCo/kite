namespace RecipeVault.Dto.Output {
    public class OnboardingStatusDto {
        public bool HasCompletedOnboarding { get; set; }
        public int RecipeCount { get; set; }
        public bool HasDietaryProfile { get; set; }
        public bool HasImportedRecipes { get; set; }
        public OnboardingProgressDto Progress { get; set; }
    }
}
