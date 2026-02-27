import { RecipeIngredient, RecipeInstruction, RecipeTag } from './recipe.model';

export interface RecipeLink {
  recipeLinkResourceId: string;
  parentRecipeId: number;
  linkedRecipeId: number;
  ingredientIndex?: number;
  displayText?: string;
  includeInTotalTime: boolean;
  portionUsed?: number;
}

export interface LinkedRecipe {
  recipeLinkResourceId: string;
  recipeResourceId: string;
  title: string;
  yield: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  totalTimeMinutes?: number;
  originalImageUrl?: string;
  ingredientIndex?: number;
  displayText?: string;
  includeInTotalTime: boolean;
  portionUsed?: number;
  ingredients?: RecipeIngredient[];
  instructions?: RecipeInstruction[];
  tags?: RecipeTag[];
}

export interface UsedInRecipe {
  recipeLinkResourceId: string;
  recipeResourceId: string;
  title: string;
  originalImageUrl?: string;
  ownerName: string;
}

export interface CreateRecipeLinkRequest {
  linkedRecipeResourceId: string;
  ingredientIndex?: number;
  displayText?: string;
  includeInTotalTime: boolean;
  portionUsed?: number;
}

export interface UpdateRecipeLinkRequest {
  ingredientIndex?: number;
  displayText?: string;
  includeInTotalTime: boolean;
  portionUsed?: number;
}
