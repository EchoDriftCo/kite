export interface IngredientSearchRequest {
  ingredients: string[];
  maxMissingIngredients: number;
  includePantryStaples: boolean;
  includeSubstitutions: boolean;
  maxCookTimeMinutes?: number;
  dietaryProfileResourceId?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: 'matchPercentage' | 'rating' | 'cookTime';
}

export interface IngredientSearchResult {
  recipe: RecipeSummary;
  matchedIngredients: string[];
  missingIngredients: string[];
  pantryStaplesUsed: string[];
  matchPercentage: number;
  weightedMatchPercentage: number;
  substitutionsAvailable?: SubstitutionSuggestion[];
}

export interface RecipeSummary {
  recipeResourceId: string;
  title: string;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  totalTimeMinutes?: number;
  originalImageUrl?: string;
  rating?: number;
  isFavorite?: boolean;
  isOwner?: boolean;
  tags?: RecipeTag[];
}

export interface SubstitutionSuggestion {
  recipeIngredient: string;
  possibleSubstitute: string;
}

export interface IngredientSuggestion {
  name: string;
  recipeCount: number;
}

export interface UserPantryItem {
  userPantryItemId?: number;
  ingredientName: string;
  isStaple: boolean;
  expirationDate?: string;
}

export interface CreatePantryItem {
  ingredientName: string;
  isStaple: boolean;
  expirationDate?: string;
}

export interface UpdatePantryItem {
  ingredientName: string;
  isStaple: boolean;
  expirationDate?: string;
}

export interface RecipeTag {
  name: string;
  category?: number;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalItems: number;
}
