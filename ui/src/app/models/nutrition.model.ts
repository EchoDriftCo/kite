export interface RecipeNutrition {
  recipeNutritionId: number;
  recipeId: number;
  caloriesPerServing?: number;
  proteinPerServing?: number;
  carbsPerServing?: number;
  fatPerServing?: number;
  fiberPerServing?: number;
  sugarPerServing?: number;
  sodiumPerServing?: number;
  ingredientsMatched: number;
  ingredientsTotal: number;
  coveragePercent: number;
  calculatedDate: string;
  isStale: boolean;
}

export interface IngredientNutrition {
  ingredientNutritionId: number;
  recipeIngredientId: number;
  fdcId?: number;
  matchedFoodName: string;
  matchConfidence: number;
  calories?: number;
  proteinGrams?: number;
  carbsGrams?: number;
  fatGrams?: number;
  fiberGrams?: number;
  sugarGrams?: number;
  sodiumMg?: number;
  gramsUsed: number;
  calculatedDate: string;
  isManualOverride: boolean;
}

export interface FoodSearchResult {
  fdcId: number;
  description: string;
  dataType: string;
  brandOwner?: string;
  nutrients: NutrientInfo[];
}

export interface NutrientInfo {
  nutrientId: number;
  name: string;
  number: string;
  unit: string;
  value: number;
}

export interface UpdateIngredientNutritionRequest {
  fdcId?: number;
  matchedFoodName: string;
  matchConfidence: number;
}
