// Models matching backend API structure

export interface RecipeIngredient {
  recipeIngredientId?: number;
  sortOrder: number;
  quantity?: number;
  unit?: string;
  item: string;
  preparation?: string;
  rawText?: string;
}

export interface RecipeInstruction {
  recipeInstructionId?: number;
  stepNumber: number;
  instruction: string;
  rawText?: string;
}

export interface Recipe {
  recipeResourceId: string;
  title: string;
  description?: string;
  yield: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  totalTimeMinutes?: number;
  source?: string;
  originalImageUrl?: string;
  ingredients: RecipeIngredient[];
  instructions: RecipeInstruction[];
  createdBy?: string;
  createdDate?: string;
  lastModifiedBy?: string;
  lastModifiedDate?: string;
}

export interface CreateRecipeRequest {
  title: string;
  description?: string;
  yield: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  totalTimeMinutes?: number;
  source?: string;
  originalImageUrl?: string;
  ingredients: RecipeIngredient[];
  instructions: RecipeInstruction[];
}

export interface UpdateRecipeRequest extends CreateRecipeRequest {
  // Same as create for now
}

export interface RecipeSearchRequest {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  title?: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ParseRecipeRequest {
  imageUrl?: string;
  imageData?: string;
  recipeText?: string;
}

export interface ParseRecipeResponse {
  recipe: CreateRecipeRequest;
  confidence?: number;
  warnings?: string[];
}
