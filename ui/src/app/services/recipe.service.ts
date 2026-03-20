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
  ParseRecipeResponse,
  PaprikaImportResult
} from '../models/recipe.model';
import { AssignTagsRequest } from '../models/tag.model';

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
    if (request.isPublic != null) params.isPublic = request.isPublic;
    if (request.includePublic != null) params.includePublic = request.includePublic;
    if (request.tagCategory != null) params.tagCategory = request.tagCategory;
    if (request.isFavorite != null) params.isFavorite = request.isFavorite;
    if (request.minRating != null) params.minRating = request.minRating;

    // Handle array parameters for tag resource IDs
    let queryString = Object.keys(params)
      .map(key => `${key}=${encodeURIComponent(params[key])}`)
      .join('&');

    if (request.tagResourceIds && request.tagResourceIds.length > 0) {
      const tagParams = request.tagResourceIds
        .map(id => `tagResourceIds=${encodeURIComponent(id)}`)
        .join('&');
      queryString = queryString ? `${queryString}&${tagParams}` : tagParams;
    }

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
   * Set recipe visibility (public/private)
   */
  setVisibility(id: string, isPublic: boolean): Observable<Recipe> {
    return this.api.put<Recipe>(`${this.endpoint}/${id}/visibility`, { isPublic });
  }

  /**
   * Parse a recipe from image or URL using AI (Gemini)
   */
  parseRecipe(request: ParseRecipeRequest): Observable<ParseRecipeResponse> {
    if (request.recipeText) {
      // Browser-extension fallback parsing from already-captured page HTML/text
      return this.api.post<ParseRecipeResponse>(`${this.endpoint}/parse`, {
        html: request.recipeText,
        url: request.imageUrl
      });
    }

    if (request.imageUrl) {
      // URL-based parsing
      return this.api.post<ParseRecipeResponse>(`${this.endpoint}/parse`, {
        url: request.imageUrl
      });
    }

    // Image-based parsing
    return this.api.post<ParseRecipeResponse>(`${this.endpoint}/parse`, {
      image: request.imageData,
      mimeType: request.mimeType || 'image/jpeg'
    });
  }

  /**
   * Upload a recipe image and get back a URL
   */
  uploadImage(imageData: string, mimeType: string): Observable<{ url: string }> {
    return this.api.post<{ url: string }>(`${this.endpoint}/images`, { imageData, mimeType });
  }

  /**
   * Assign tags to a recipe
   */
  assignTags(recipeId: string, request: AssignTagsRequest): Observable<Recipe> {
    return this.api.post<Recipe>(`${this.endpoint}/${recipeId}/tags`, request);
  }

  /**
   * Remove a tag from a recipe
   */
  removeTag(recipeId: string, tagId: string): Observable<Recipe> {
    return this.api.delete<Recipe>(`${this.endpoint}/${recipeId}/tags/${tagId}`);
  }

  /**
   * Trigger AI dietary tag analysis for a recipe
   */
  analyzeDietaryTags(recipeId: string): Observable<Recipe> {
    return this.api.post<Recipe>(`${this.endpoint}/${recipeId}/analyze-tags`, {});
  }

  setRating(id: string, rating: number | null): Observable<Recipe> {
    return this.api.put<Recipe>(`${this.endpoint}/${id}/rating`, { rating });
  }

  setFavorite(id: string, isFavorite: boolean): Observable<Recipe> {
    return this.api.put<Recipe>(`${this.endpoint}/${id}/favorite`, { isFavorite });
  }

  generateShareToken(id: string): Observable<Recipe> {
    return this.api.post<Recipe>(`${this.endpoint}/${id}/share-token`, {});
  }

  revokeShareToken(id: string): Observable<Recipe> {
    return this.api.delete<Recipe>(`${this.endpoint}/${id}/share-token`);
  }

  getSharedRecipe(token: string): Observable<Recipe> {
    return this.api.get<Recipe>(`${this.endpoint}/shared/${token}`);
  }

  /**
   * Discover public recipes (popular/newest/rating)
   */
  discoverRecipes(request: { title?: string; sortBy?: string; tagResourceIds?: string[]; minRating?: number; pageNumber?: number; pageSize?: number } = {}): Observable<PagedResult<Recipe>> {
    const params: any = {};
    if (request.pageNumber) params.pageNumber = request.pageNumber;
    if (request.pageSize) params.pageSize = request.pageSize;
    if (request.title) params.title = request.title;
    if (request.sortBy) params.sortBy = request.sortBy;
    if (request.minRating != null) params.minRating = request.minRating;

    let queryString = Object.keys(params)
      .map(key => `${key}=${encodeURIComponent(params[key])}`)
      .join('&');

    if (request.tagResourceIds && request.tagResourceIds.length > 0) {
      const tagParams = request.tagResourceIds
        .map(id => `tagResourceIds=${encodeURIComponent(id)}`)
        .join('&');
      queryString = queryString ? `${queryString}&${tagParams}` : tagParams;
    }

    const url = queryString ? `${this.endpoint}/discover?${queryString}` : `${this.endpoint}/discover`;
    return this.api.get<PagedResult<Recipe>>(url);
  }

  /**
   * Fork a recipe to create a personal copy
   */
  forkRecipe(id: string, title?: string): Observable<Recipe> {
    const body = title ? { title } : {};
    return this.api.post<Recipe>(`${this.endpoint}/${id}/fork`, body);
  }

  /**
   * Import recipes from Paprika (.paprikarecipes file)
   */
  importFromPaprika(file: File): Observable<PaprikaImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.api.post<PaprikaImportResult>('import/paprika', formData);
  }

  /**
   * Import recipe from multiple images (1-4 images, processed sequentially)
   */
  importFromMultipleImages(formData: FormData): Observable<Recipe> {
    return this.api.post<Recipe>('import/multi-image', formData);
  }

  /**
   * Import recipe from video (TikTok, Instagram Reels, YouTube)
   */
  importFromVideo(url: string): Promise<any> {
    return this.api.post<any>('import/video', { 
      url: url,
      includeSubtitles: true 
    }).toPromise();
  }
}
