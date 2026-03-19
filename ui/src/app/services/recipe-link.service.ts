import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  RecipeLink,
  LinkedRecipe,
  UsedInRecipe,
  CreateRecipeLinkRequest,
  UpdateRecipeLinkRequest
} from '../models/recipe-link.model';
import { Recipe } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class RecipeLinkService {
  constructor(private api: ApiService) {}

  /**
   * Create a link to another recipe
   */
  createRecipeLink(parentRecipeId: string, request: CreateRecipeLinkRequest): Observable<RecipeLink> {
    return this.api.post<RecipeLink>(`recipes/${parentRecipeId}/links`, request);
  }

  /**
   * Update a recipe link
   */
  updateRecipeLink(parentRecipeId: string, linkId: string, request: UpdateRecipeLinkRequest): Observable<RecipeLink> {
    return this.api.put<RecipeLink>(`recipes/${parentRecipeId}/links/${linkId}`, request);
  }

  /**
   * Delete a recipe link
   */
  deleteRecipeLink(parentRecipeId: string, linkId: string): Observable<void> {
    return this.api.delete<void>(`recipes/${parentRecipeId}/links/${linkId}`);
  }

  /**
   * Get linked recipes (components) for a recipe
   */
  getLinkedRecipes(parentRecipeId: string): Observable<LinkedRecipe[]> {
    return this.api.get<LinkedRecipe[]>(`recipes/${parentRecipeId}/links`);
  }

  /**
   * Get recipes that use this recipe as a component
   */
  getUsedInRecipes(recipeId: string): Observable<UsedInRecipe[]> {
    return this.api.get<UsedInRecipe[]>(`recipes/${recipeId}/used-in`);
  }

  /**
   * Search user's recipes for linking candidates
   */
  searchLinkableRecipes(query: string): Observable<Recipe[]> {
    const url = query ? `recipes/linkable?query=${encodeURIComponent(query)}` : 'recipes/linkable';
    return this.api.get<Recipe[]>(url);
  }
}
