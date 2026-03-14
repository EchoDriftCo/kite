export interface OnboardingStatus {
  hasCompletedOnboarding: boolean;
  recipeCount: number;
  hasDietaryProfile: boolean;
  hasImportedRecipes: boolean;
  progress: OnboardingProgress;
}

export interface OnboardingProgress {
  dietaryProfileSet: boolean;
  samplesAdded: boolean;
  tourCompleted: boolean;
}

export interface AddSampleRecipesResult {
  recipesAdded: number;
  recipes: AddedRecipe[];
}

export interface AddedRecipe {
  recipeResourceId: string;
  title: string;
  showcases: string;
}

export interface RemoveSampleRecipesResult {
  recipesRemoved: number;
}
