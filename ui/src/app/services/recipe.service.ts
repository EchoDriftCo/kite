import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Recipe,
  CreateRecipeRequest,
  UpdateRecipeRequest,
  RecipeSearchRequest,
  PagedResult,
  ParseRecipeRequest,
  ParseRecipeResponse
} from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  private readonly endpoint = 'recipes';

  constructor(private api: ApiService) {}

  /**
   * Search/list recipes with pagination
   */
  searchRecipes(request: RecipeSearchRequest = {}): Observable<PagedResult<Recipe>> {
    const params: any = {};
    
    if (request.pageNumber) params.pageNumber = request.pageNumber;
    if (request.pageSize) params.pageSize = request.pageSize || 20;
    if (request.sortBy) params.sortBy = request.sortBy;
    if (request.sortDirection) params.sortDirection = request.sortDirection;
    if (request.title) params.title = request.title;

    // Build query string
    const queryString = Object.keys(params)
      .map(key => `${key}=${encodeURIComponent(params[key])}`)
      .join('&');

    const url = queryString ? `${this.endpoint}?${queryString}` : this.endpoint;
    
    return this.api.get<PagedResult<Recipe>>(url);
  }

  /**
   * Get a single recipe by ID
   */
  getRecipe(id: string): Observable<Recipe> {
    return this.api.get<Recipe>(`${this.endpoint}/${id}`);
  }

  /**
   * Create a new recipe
   */
  createRecipe(recipe: CreateRecipeRequest): Observable<Recipe> {
    return this.api.post<Recipe>(this.endpoint, recipe);
  }

  /**
   * Update an existing recipe
   */
  updateRecipe(id: string, recipe: UpdateRecipeRequest): Observable<Recipe> {
    return this.api.put<Recipe>(`${this.endpoint}/${id}`, recipe);
  }

  /**
   * Delete a recipe
   */
  deleteRecipe(id: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${id}`);
  }

  /**
   * Parse a recipe from image using AI (Gemini)
   */
  parseRecipe(request: ParseRecipeRequest): Observable<ParseRecipeResponse> {
    // Backend expects { image: base64, mimeType: string }
    const payload = {
      image: request.imageData,
      mimeType: 'image/jpeg'  // Default, could be determined from data URL prefix
    };
    return this.api.post<ParseRecipeResponse>(`${this.endpoint}/parse`, payload);
  }
}
