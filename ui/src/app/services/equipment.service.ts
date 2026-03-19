import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Equipment,
  UserEquipment,
  RecipeEquipment,
  AddEquipmentRequest,
  EquipmentCheckResult
} from '../models/equipment.model';

@Injectable({
  providedIn: 'root'
})
export class EquipmentService {
  private readonly endpoint = 'api/v1/equipment';

  constructor(private api: ApiService) {}

  /**
   * Get all available equipment
   */
  getAllEquipment(): Observable<Equipment[]> {
    return this.api.get<Equipment[]>(this.endpoint);
  }

  /**
   * Get user's owned equipment
   */
  getMyEquipment(): Observable<UserEquipment[]> {
    return this.api.get<UserEquipment[]>(`${this.endpoint}/my`);
  }

  /**
   * Add equipment to user's collection
   */
  addEquipment(request: AddEquipmentRequest): Observable<UserEquipment> {
    return this.api.post<UserEquipment>(`${this.endpoint}/my`, request);
  }

  /**
   * Remove equipment from user's collection
   */
  removeEquipment(code: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/my/${code}`);
  }

  /**
   * Get equipment needed for a recipe
   */
  getRecipeEquipment(recipeId: string): Observable<RecipeEquipment[]> {
    return this.api.get<RecipeEquipment[]>(`${this.endpoint}/recipes/${recipeId}`);
  }

  /**
   * Auto-detect equipment from recipe instructions
   */
  detectRecipeEquipment(recipeId: string): Observable<RecipeEquipment[]> {
    return this.api.post<RecipeEquipment[]>(`${this.endpoint}/recipes/${recipeId}/detect`, {});
  }

  /**
   * Check if user has all equipment needed for a recipe
   */
  checkRecipeEquipment(recipeId: string): Observable<EquipmentCheckResult> {
    return this.api.get<EquipmentCheckResult>(`${this.endpoint}/recipes/${recipeId}/check`);
  }
}
