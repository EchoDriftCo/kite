export interface CookingLogEntry {
  cookingLogId: number;
  recipeId: number;
  recipeResourceId: string;
  recipeTitle: string;
  cookedDate: string; // ISO string
  scaleFactor?: number;
  servingsMade?: number;
  notes?: string;
  rating?: number; // 1-5
  photoUrls?: string[];
  createdBy: string;
  createdDate: string;
  lastModifiedBy?: string;
  lastModifiedDate?: string;
}

export interface CreateCookingLogRequest {
  recipeId: string; // resource ID
  cookedDate: string; // ISO string
  scaleFactor?: number;
  servingsMade?: number;
  notes?: string;
  rating?: number; // 1-5
}

export interface UpdateCookingLogRequest {
  cookedDate?: string;
  scaleFactor?: number;
  servingsMade?: number;
  notes?: string;
  rating?: number;
}

export interface CookingStats {
  totalCooks: number;
  uniqueRecipes: number;
  currentStreak: number;
  longestStreak: number;
  mostCooked: MostCookedRecipe[];
  cooksThisMonth: number;
  averageCooksPerWeek: number;
  mostActiveDayOfWeek: string;
}

export interface MostCookedRecipe {
  recipeResourceId: string;
  recipeTitle: string;
  cookCount: number;
}

export interface CalendarDay {
  date: string; // YYYY-MM-DD
  cookCount: number;
  entries: CookingLogEntry[];
}

export interface CalendarResponse {
  year: number;
  month: number;
  days: CalendarDay[];
}

export interface RecipePersonalStats {
  recipeResourceId: string;
  cookCount: number;
  averageRating?: number;
  lastCooked?: string; // ISO string
  lastNote?: string;
}
