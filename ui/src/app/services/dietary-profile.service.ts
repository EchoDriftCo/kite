import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DietaryProfile,
  UpdateDietaryProfile,
  AddDietaryRestriction,
  AddAvoidedIngredient,
  DietaryConflictCheck
} from '../models/dietary-profile.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DietaryProfileService {
  private apiUrl = `${environment.apiUrl}/api/v1/dietary-profiles`;

  constructor(private http: HttpClient) { }

  getProfiles(): Observable<DietaryProfile[]> {
    return this.http.get<DietaryProfile[]>(this.apiUrl);
  }

  getProfile(id: string): Observable<DietaryProfile> {
    return this.http.get<DietaryProfile>(`${this.apiUrl}/${id}`);
  }

  createProfile(profile: UpdateDietaryProfile): Observable<DietaryProfile> {
    return this.http.post<DietaryProfile>(this.apiUrl, profile);
  }

  updateProfile(id: string, profile: UpdateDietaryProfile): Observable<DietaryProfile> {
    return this.http.put<DietaryProfile>(`${this.apiUrl}/${id}`, profile);
  }

  deleteProfile(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  addRestriction(profileId: string, restriction: AddDietaryRestriction): Observable<DietaryProfile> {
    return this.http.post<DietaryProfile>(`${this.apiUrl}/${profileId}/restrictions`, restriction);
  }

  removeRestriction(profileId: string, restrictionCode: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${profileId}/restrictions/${restrictionCode}`);
  }

  addAvoidedIngredient(profileId: string, ingredient: AddAvoidedIngredient): Observable<DietaryProfile> {
    return this.http.post<DietaryProfile>(`${this.apiUrl}/${profileId}/avoided-ingredients`, ingredient);
  }

  removeAvoidedIngredient(profileId: string, ingredientId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${profileId}/avoided-ingredients/${ingredientId}`);
  }

  checkRecipe(recipeId: string, profileId?: string): Observable<DietaryConflictCheck> {
    const url = `${environment.apiUrl}/api/v1/recipes/${recipeId}/dietary-check`;
    const params = profileId ? { profileId } : {};
    return this.http.get<DietaryConflictCheck>(url, { params });
  }
}
