import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  RecipeNutrition,
  IngredientNutrition,
  FoodSearchResult,
  UpdateIngredientNutritionRequest
} from '../models/nutrition.model';

@Injectable({
  providedIn: 'root'
})
export class NutritionService {

  constructor(private api: ApiService) {}

  /**
   * Trigger nutrition analysis for a recipe
   */
  analyzeRecipeNutrition(recipeResourceId: string): Observable<RecipeNutrition> {
    return this.api.post<RecipeNutrition>(
      `recipes/${recipeResourceId}/nutrition/analyze`,
      {}
    );
  }

  /**
   * Get cached nutrition data for a recipe
   */
  getRecipeNutrition(recipeResourceId: string): Observable<RecipeNutrition> {
    return this.api.get<RecipeNutrition>(
      `recipes/${recipeResourceId}/nutrition`
    );
  }

  /**
   * Update ingredient nutrition (manual override)
   */
  updateIngredientNutrition(
    recipeResourceId: string,
    ingredientIndex: number,
    request: UpdateIngredientNutritionRequest
  ): Observable<IngredientNutrition> {
    return this.api.put<IngredientNutrition>(
      `recipes/${recipeResourceId}/ingredients/${ingredientIndex}/nutrition`,
      request
    );
  }

  /**
   * Search USDA FoodData Central
   */
  searchFoods(query: string): Observable<FoodSearchResult[]> {
    return this.api.get<FoodSearchResult[]>(
      `nutrition/search?query=${encodeURIComponent(query)}`
    );
  }
}
