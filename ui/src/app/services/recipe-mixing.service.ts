import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface MixRecipesRequest {
  recipeAId: string;
  recipeBId: string;
  intent?: string;
  mode: 'guided' | 'surprise' | 'bestOfBoth';
}

export interface MixedIngredient {
  quantity?: number;
  unit?: string;
  item: string;
  preparation?: string;
  rawText?: string;
  attribution?: string;
}

export interface MixedInstruction {
  stepNumber: number;
  instruction: string;
  rawText?: string;
  attribution?: string;
}

export interface MixedRecipePreview {
  title: string;
  description?: string;
  yield: number;
  prepTimeMinutes?: number;
  cookTimeMinutes?: number;
  ingredients: MixedIngredient[];
  instructions: MixedInstruction[];
  mixNotes?: string;
  source?: string;
  recipeAResourceId?: string;
  recipeBResourceId?: string;
}

export interface RefineMixRequest {
  preview: MixedRecipePreview;
  refinementNotes: string;
}

@Injectable({
  providedIn: 'root'
})
export class RecipeMixingService {
  private baseUrl = `${environment.apiUrl}/recipes/mix`;

  constructor(private http: HttpClient) {}

  /**
   * Mix two recipes together using AI
   */
  mixRecipes(request: MixRecipesRequest): Observable<MixedRecipePreview> {
    return this.http.post<MixedRecipePreview>(this.baseUrl, request);
  }

  /**
   * Refine a mixed recipe based on user feedback
   */
  refineMix(request: RefineMixRequest): Observable<MixedRecipePreview> {
    return this.http.post<MixedRecipePreview>(`${this.baseUrl}/refine`, request);
  }

  /**
   * Save a mixed recipe preview as a real recipe
   */
  saveMixedRecipe(preview: MixedRecipePreview): Observable<any> {
    return this.http.post(`${this.baseUrl}/save`, preview);
  }
}
