import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  SubstitutionRequest,
  SubstitutionResponse,
  ApplySubstitutionsRequest,
  SubstitutionSelection
} from '../models/substitution.model';
import { Recipe } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class SubstitutionService {
  constructor(private api: ApiService) {}

  /**
   * Get AI-powered substitution suggestions for a recipe
   * @param recipeId The recipe resource ID
   * @param ingredientIndices Indices of specific ingredients to substitute (optional)
   * @param dietaryConstraints Dietary constraints to apply (optional)
   * @returns Observable of substitution suggestions
   */
  getSubstitutions(
    recipeId: string,
    ingredientIndices?: number[],
    dietaryConstraints?: string[]
  ): Observable<SubstitutionResponse> {
    const request: SubstitutionRequest = {};

    if (ingredientIndices && ingredientIndices.length > 0) {
      request.ingredientIndices = ingredientIndices;
    }

    if (dietaryConstraints && dietaryConstraints.length > 0) {
      request.dietaryConstraints = dietaryConstraints;
    }

    return this.api.post<SubstitutionResponse>(
      `recipes/${recipeId}/substitutions`,
      request
    );
  }

  /**
   * Apply selected substitutions and create a forked recipe
   * @param recipeId The recipe resource ID
   * @param selections The user's selections for which substitutions to apply
   * @param forkTitle Optional custom title for the forked recipe
   * @returns Observable of the newly forked recipe
   */
  applySubstitutions(
    recipeId: string,
    selections: SubstitutionSelection[],
    forkTitle?: string
  ): Observable<Recipe> {
    const request: ApplySubstitutionsRequest = {
      selections
    };

    if (forkTitle) {
      request.forkTitle = forkTitle;
    }

    return this.api.post<Recipe>(
      `recipes/${recipeId}/substitutions/apply`,
      request
    );
  }
}
