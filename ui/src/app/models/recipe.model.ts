export interface Recipe {
  id: string;
  name: string;
  description?: string;
  ingredients?: string[];
  instructions?: string;
  prepTime?: number;
  cookTime?: number;
  servings?: number;
  category?: string;
  originalImageUrl?: string;
  thumbnailUrl?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface CreateRecipeRequest {
  name: string;
  description?: string;
  ingredients?: string[];
  instructions?: string;
  prepTime?: number;
  cookTime?: number;
  servings?: number;
  category?: string;
  imageFile?: File;
}

export interface UpdateRecipeRequest extends CreateRecipeRequest {
  id: string;
}
