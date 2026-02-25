import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
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
  private readonly apiUrl = `${environment.apiBaseUrl}/api/v1`;

  constructor(private http: HttpClient) {}

  /**
   * Trigger nutrition analysis for a recipe
   */
  analyzeRecipeNutrition(recipeResourceId: string): Observable<RecipeNutrition> {
    return this.http.post<RecipeNutrition>(
      `${this.apiUrl}/recipes/${recipeResourceId}/nutrition/analyze`,
      {}
    );
  }

  /**
   * Get cached nutrition data for a recipe
   */
  getRecipeNutrition(recipeResourceId: string): Observable<RecipeNutrition> {
    return this.http.get<RecipeNutrition>(
      `${this.apiUrl}/recipes/${recipeResourceId}/nutrition`
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
    return this.http.put<IngredientNutrition>(
      `${this.apiUrl}/recipes/${recipeResourceId}/ingredients/${ingredientIndex}/nutrition`,
      request
    );
  }

  /**
   * Search USDA FoodData Central
   */
  searchFoods(query: string): Observable<FoodSearchResult[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<FoodSearchResult[]>(
      `${this.apiUrl}/nutrition/search`,
      { params }
    );
  }
}
