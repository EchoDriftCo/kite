import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import {
  IngredientSearchRequest,
  IngredientSearchResult,
  IngredientSuggestion,
  UserPantryItem,
  CreatePantryItem,
  UpdatePantryItem,
  PagedResult
} from '../models/ingredient-search.model';

@Injectable({
  providedIn: 'root'
})
export class IngredientSearchService {
  private readonly searchEndpoint = 'recipes/search/by-ingredients';
  private readonly suggestEndpoint = 'ingredients/suggest';
  private readonly pantryEndpoint = 'pantry';

  // Client-side cache for autocomplete suggestions
  private suggestionCache = new Map<string, { suggestions: IngredientSuggestion[], timestamp: number }>();
  private readonly CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes

  constructor(private api: ApiService) {}

  // =========================================================================
  // Ingredient Search
  // =========================================================================

  searchByIngredients(request: IngredientSearchRequest): Observable<PagedResult<IngredientSearchResult>> {
    return this.api.post<PagedResult<IngredientSearchResult>>(this.searchEndpoint, request);
  }

  // =========================================================================
  // Autocomplete Suggestions (with caching)
  // =========================================================================

  suggestIngredients(query: string, limit: number = 10): Observable<IngredientSuggestion[]> {
    const normalizedQuery = query.toLowerCase().trim();

    // Check cache
    const cached = this.suggestionCache.get(normalizedQuery);
    if (cached && Date.now() - cached.timestamp < this.CACHE_TTL_MS) {
      return of(cached.suggestions);
    }

    // Cache miss or expired — fetch from API
    return this.api.get<{ suggestions: IngredientSuggestion[] }>(
      `${this.suggestEndpoint}?query=${encodeURIComponent(normalizedQuery)}&limit=${limit}`
    ).pipe(
      map(response => response.suggestions),
      tap(suggestions => {
        this.suggestionCache.set(normalizedQuery, {
          suggestions,
          timestamp: Date.now()
        });
        this.pruneCache();
      })
    );
  }

  private pruneCache(): void {
    const now = Date.now();
    const keysToDelete: string[] = [];

    this.suggestionCache.forEach((value, key) => {
      if (now - value.timestamp >= this.CACHE_TTL_MS) {
        keysToDelete.push(key);
      }
    });

    keysToDelete.forEach(key => this.suggestionCache.delete(key));
  }

  clearSuggestionCache(): void {
    this.suggestionCache.clear();
  }

  // =========================================================================
  // User Pantry Management
  // =========================================================================

  getUserPantry(): Observable<UserPantryItem[]> {
    return this.api.get<{ items: UserPantryItem[] }>(this.pantryEndpoint)
      .pipe(map(response => response.items));
  }

  addPantryItem(item: CreatePantryItem): Observable<UserPantryItem> {
    return this.api.post<UserPantryItem>(this.pantryEndpoint, item);
  }

  updatePantryItem(id: number, item: UpdatePantryItem): Observable<UserPantryItem> {
    return this.api.put<UserPantryItem>(`${this.pantryEndpoint}/${id}`, item);
  }

  deletePantryItem(id: number): Observable<void> {
    return this.api.delete<void>(`${this.pantryEndpoint}/${id}`);
  }

  getDefaultStaples(): Observable<string[]> {
    return this.api.get<string[]>(`${this.pantryEndpoint}/defaults`);
  }
}
