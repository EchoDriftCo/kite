// Tag models matching backend API structure

export enum TagCategory {
  Dietary = 1,
  Cuisine = 2,
  MealType = 3,
  Source = 4,
  Custom = 5
}

export interface Tag {
  tagResourceId: string;
  tagId: number;
  name: string;
  category: number;
  categoryName: string;
  isGlobal: boolean;
  createdBy?: string;
  createdDate?: string;
  lastModifiedBy?: string;
  lastModifiedDate?: string;
}

export interface RecipeTag {
  tagResourceId: string;
  name: string;
  category: number;
  categoryName: string;
  isAiAssigned: boolean;
  confidence?: number;
  isOverridden: boolean;
}

export interface CreateTagRequest {
  name: string;
  category: number;
}

export interface UpdateTagRequest {
  name: string;
  category: number;
}

export interface TagSearchRequest {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  name?: string;
  category?: number;
  isGlobal?: boolean;
}

export interface AssignTagItem {
  tagResourceId?: string;  // For existing tags
  name?: string;           // For creating new tags
  category?: number;       // Required when creating new tags
}

export interface AssignTagsRequest {
  tags: AssignTagItem[];
}

export function getCategoryName(category: number): string {
  switch (category) {
    case TagCategory.Dietary:
      return 'Dietary';
    case TagCategory.Cuisine:
      return 'Cuisine';
    case TagCategory.MealType:
      return 'Meal Type';
    case TagCategory.Source:
      return 'Source';
    case TagCategory.Custom:
      return 'Custom';
    default:
      return 'Unknown';
  }
}

export function getCategoryColor(category: number): string {
  switch (category) {
    case TagCategory.Dietary:
      return 'success';
    case TagCategory.Cuisine:
      return 'primary';
    case TagCategory.MealType:
      return 'accent';
    case TagCategory.Source:
      return 'warn';
    case TagCategory.Custom:
      return '';
    default:
      return '';
  }
}
