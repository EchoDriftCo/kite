import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Recipe, CreateRecipeRequest, UpdateRecipeRequest } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  private endpoint = 'recipes';

  constructor(private apiService: ApiService) { }

  getAllRecipes(): Observable<Recipe[]> {
    return this.apiService.get<Recipe[]>(this.endpoint);
  }

  getRecipeById(id: string): Observable<Recipe> {
    return this.apiService.get<Recipe>(`${this.endpoint}/${id}`);
  }

  createRecipe(recipe: CreateRecipeRequest): Observable<Recipe> {
    return this.apiService.post<Recipe>(this.endpoint, recipe);
  }

  updateRecipe(recipe: UpdateRecipeRequest): Observable<Recipe> {
    return this.apiService.put<Recipe>(`${this.endpoint}/${recipe.id}`, recipe);
  }

  deleteRecipe(id: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${id}`);
  }

  uploadRecipeImage(recipeId: string, imageFile: File): Observable<Recipe> {
    const formData = new FormData();
    formData.append('image', imageFile);
    return this.apiService.upload<Recipe>(`${this.endpoint}/${recipeId}/image`, formData);
  }

  parseRecipeFromImage(imageFile: File): Observable<Recipe> {
    const formData = new FormData();
    formData.append('image', imageFile);
    return this.apiService.upload<Recipe>(`${this.endpoint}/parse`, formData);
  }
}
