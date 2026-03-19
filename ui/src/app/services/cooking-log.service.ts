import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  CookingLogEntry,
  CreateCookingLogRequest,
  UpdateCookingLogRequest,
  CookingStats,
  CalendarResponse,
  RecipePersonalStats
} from '../models/cooking-log.model';
import { PagedResult } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class CookingLogService {
  private readonly endpoint = 'cooking-log';

  constructor(private api: ApiService) {}

  /**
   * Log a new cook
   */
  createLog(request: CreateCookingLogRequest): Observable<CookingLogEntry> {
    return this.api.post<CookingLogEntry>(this.endpoint, request);
  }

  /**
   * Get paginated cooking history
   */
  getHistory(pageNumber = 1, pageSize = 20): Observable<PagedResult<CookingLogEntry>> {
    return this.api.get<PagedResult<CookingLogEntry>>(
      `${this.endpoint}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  }

  /**
   * Get a specific log entry
   */
  getLog(id: number): Observable<CookingLogEntry> {
    return this.api.get<CookingLogEntry>(`${this.endpoint}/${id}`);
  }

  /**
   * Update a log entry
   */
  updateLog(id: number, request: UpdateCookingLogRequest): Observable<CookingLogEntry> {
    return this.api.put<CookingLogEntry>(`${this.endpoint}/${id}`, request);
  }

  /**
   * Delete a log entry
   */
  deleteLog(id: number): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${id}`);
  }

  /**
   * Add a photo to a log entry
   */
  addPhoto(id: number, photoData: string, mimeType: string): Observable<CookingLogEntry> {
    return this.api.post<CookingLogEntry>(`${this.endpoint}/${id}/photos`, {
      imageData: photoData,
      mimeType
    });
  }

  /**
   * Get user cooking stats
   */
  getStats(): Observable<CookingStats> {
    return this.api.get<CookingStats>(`${this.endpoint}/stats`);
  }

  /**
   * Get calendar view for a specific month
   */
  getCalendar(year: number, month: number): Observable<CalendarResponse> {
    return this.api.get<CalendarResponse>(`${this.endpoint}/calendar?year=${year}&month=${month}`);
  }

  /**
   * Get cooking history for a specific recipe
   */
  getRecipeHistory(recipeId: string): Observable<CookingLogEntry[]> {
    return this.api.get<CookingLogEntry[]>(`recipes/${recipeId}/cooking-log`);
  }

  /**
   * Get personal stats for a specific recipe
   */
  getRecipeStats(recipeId: string): Observable<RecipePersonalStats> {
    return this.api.get<RecipePersonalStats>(`recipes/${recipeId}/personal-stats`);
  }
}
