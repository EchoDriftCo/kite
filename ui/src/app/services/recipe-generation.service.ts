import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  GenerateRecipeRequest,
  RefineRecipeRequest,
  GeneratedRecipe,
  GenerationQuota
} from '../models/recipe-generation.model';
import { Recipe } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class RecipeGenerationService {
  constructor(private api: ApiService) {}

  /**
   * Generate new recipes from natural language prompt
   * @param request Generation request with prompt and constraints
   * @returns Observable of generated recipe(s)
   */
  generateRecipes(request: GenerateRecipeRequest): Observable<GeneratedRecipe[]> {
    return this.api.post<GeneratedRecipe[]>('recipes/generate', request);
  }

  /**
   * Refine a previously generated recipe
   * @param request Refinement request with previous recipe and instructions
   * @returns Observable of refined recipe
   */
  refineRecipe(request: RefineRecipeRequest): Observable<GeneratedRecipe> {
    return this.api.post<GeneratedRecipe>('recipes/generate/refine', request);
  }

  /**
   * Save a generated recipe to the user's library
   * @param generatedRecipe The generated recipe to save
   * @returns Observable of saved recipe
   */
  saveGeneratedRecipe(generatedRecipe: GeneratedRecipe): Observable<Recipe> {
    return this.api.post<Recipe>('recipes/generate/save', generatedRecipe);
  }

  /**
   * Get remaining generation quota for today
   * @returns Observable of quota information
   */
  getRemainingQuota(): Observable<GenerationQuota> {
    return this.api.get<GenerationQuota>('recipes/generate/quota');
  }
}
