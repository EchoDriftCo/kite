// Models matching backend Collection API structure

export interface Collection {
  collectionResourceId: string;
  name: string;
  description?: string;
  coverImageUrl?: string;
  isPublic: boolean;
  isFeatured: boolean;
  sortOrder: number;
  recipeCount: number;
  createdDate: string;
  lastModifiedDate?: string;
  recipes?: CollectionRecipe[];
}

export interface CollectionRecipe {
  recipeResourceId: string;
  title: string;
  description?: string;
  sourceImageUrl?: string;
  sortOrder: number;
  addedDate: string;
  totalTimeMinutes?: number;
  rating?: number;
}

export interface CreateCollectionRequest {
  name: string;
  description?: string;
  coverImageUrl?: string;
  isPublic?: boolean;
}

export interface UpdateCollectionRequest {
  name: string;
  description?: string;
  coverImageUrl?: string;
  isPublic?: boolean;
}

export interface AddRecipeToCollectionRequest {
  recipeResourceId: string;
  sortOrder?: number;
}

export interface ReorderCollectionRecipesRequest {
  recipes: RecipeOrder[];
}

export interface RecipeOrder {
  recipeResourceId: string;
  sortOrder: number;
}

export interface ReorderCollectionsRequest {
  collections: CollectionOrder[];
}

export interface CollectionOrder {
  collectionResourceId: string;
  sortOrder: number;
}

export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
