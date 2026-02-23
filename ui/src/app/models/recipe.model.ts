// Models matching backend API structure
import { RecipeTag } from './tag.model';

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
  sourceImageUrl?: string;
  isPublic?: boolean;
  rating?: number;
  isFavorite?: boolean;
  isOwner?: boolean;
  shareToken?: string;
  ingredients: RecipeIngredient[];
  instructions: RecipeInstruction[];
  tags?: RecipeTag[];
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
  sourceImageUrl?: string;
  isPublic?: boolean;
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
  isPublic?: boolean;
  includePublic?: boolean;
  tagResourceIds?: string[];
  tagCategory?: number;
  isFavorite?: boolean;
  minRating?: number;
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
  imageData?: string;  // base64 encoded
  mimeType?: string;   // MIME type of the image (e.g., 'image/jpeg', 'image/png')
  recipeText?: string;
}

export interface ParseRecipeResponse {
  confidence: number;
  recipe: ParsedRecipe;
  warnings?: string[];
}

export interface ParsedRecipe {
  title?: string;
  yield?: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  ingredients?: ParsedIngredient[];
  instructions?: ParsedInstruction[];
  imageUrl?: string;
}

export interface ParsedIngredient {
  quantity?: number;
  unit?: string;
  item: string;
  preparation?: string;
  rawText?: string;
}

export interface ParsedInstruction {
  stepNumber: number;
  instruction: string;
  rawText?: string;
}

export interface ImportedRecipe {
  recipeResourceId: string;
  title: string;
}

export interface ImportError {
  recipeName: string;
  errorMessage: string;
}

export interface PaprikaImportResult {
  totalRecipes: number;
  successCount: number;
  failureCount: number;
  importedRecipes: ImportedRecipe[];
  errors: ImportError[];
}
