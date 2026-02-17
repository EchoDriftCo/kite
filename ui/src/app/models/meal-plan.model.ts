export interface MealPlan {
  mealPlanResourceId: string;
  name: string;
  startDate: string;
  endDate: string;
  entries: MealPlanEntry[];
  createdDate?: string;
  lastModifiedDate?: string;
}

export interface MealPlanEntry {
  mealPlanEntryId?: number;
  date: string;
  mealSlot: number;
  recipeResourceId: string;
  recipeTitle?: string;
  recipeYield?: number;
  servings?: number;
  isLeftover: boolean;
}

export enum MealSlot {
  Breakfast = 1,
  Lunch = 2,
  Dinner = 3,
  Snack = 4
}

export const MEAL_SLOT_LABELS: Record<number, string> = {
  [MealSlot.Breakfast]: 'Breakfast',
  [MealSlot.Lunch]: 'Lunch',
  [MealSlot.Dinner]: 'Dinner',
  [MealSlot.Snack]: 'Snack',
};

export interface CreateMealPlanRequest {
  name: string;
  startDate: string;
  endDate: string;
  entries?: MealPlanEntryRequest[];
}

export interface MealPlanEntryRequest {
  date: string;
  mealSlot: number;
  recipeResourceId: string;
  servings?: number;
  isLeftover: boolean;
}

export interface MealPlanSearchRequest {
  pageNumber?: number;
  pageSize?: number;
  startDateFrom?: string;
  startDateTo?: string;
}

export interface GroceryList {
  items: GroceryItem[];
}

export interface GroceryItem {
  item: string;
  quantity?: number;
  unit?: string;
  category?: string;
  sources?: string[];
}
