import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Collection,
  CreateCollectionRequest,
  UpdateCollectionRequest,
  AddRecipeToCollectionRequest,
  ReorderCollectionRecipesRequest,
  ReorderCollectionsRequest,
  PagedResult
} from '../models/collection.model';

@Injectable({
  providedIn: 'root'
})
export class CollectionService {
  private readonly endpoint = 'collections';

  constructor(private api: ApiService) {}

  // Collection CRUD operations

  /**
   * Create a new collection
   */
  createCollection(request: CreateCollectionRequest): Observable<Collection> {
    return this.api.post<Collection>(this.endpoint, request);
  }

  /**
   * Get all collections
   */
  getCollections(pageNumber = 1, pageSize = 50): Observable<PagedResult<Collection>> {
    const params = `pageNumber=${pageNumber}&pageSize=${pageSize}`;
    return this.api.get<PagedResult<Collection>>(`${this.endpoint}?${params}`);
  }

  /**
   * Get my collections (user's collections)
   */
  getMyCollections(): Observable<Collection[]> {
    return this.api.get<Collection[]>(`${this.endpoint}/my`);
  }

  /**
   * Get featured collections
   */
  getFeaturedCollections(pageNumber = 1, pageSize = 20): Observable<PagedResult<Collection>> {
    const params = `pageNumber=${pageNumber}&pageSize=${pageSize}`;
    return this.api.get<PagedResult<Collection>>(`${this.endpoint}/featured?${params}`);
  }

  /**
   * Get public collections (with optional search)
   */
  getPublicCollections(searchTerm?: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<Collection>> {
    let params = `pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (searchTerm) {
      params += `&search=${encodeURIComponent(searchTerm)}`;
    }
    return this.api.get<PagedResult<Collection>>(`${this.endpoint}/public?${params}`);
  }

  /**
   * Get a single collection by ID
   */
  getCollection(collectionId: string): Observable<Collection> {
    return this.api.get<Collection>(`${this.endpoint}/${collectionId}`);
  }

  /**
   * Update a collection
   */
  updateCollection(collectionId: string, request: UpdateCollectionRequest): Observable<Collection> {
    return this.api.put<Collection>(`${this.endpoint}/${collectionId}`, request);
  }

  /**
   * Delete a collection
   */
  deleteCollection(collectionId: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${collectionId}`);
  }

  // Recipe management

  /**
   * Add a recipe to a collection
   */
  addRecipeToCollection(collectionId: string, request: AddRecipeToCollectionRequest): Observable<Collection> {
    return this.api.post<Collection>(`${this.endpoint}/${collectionId}/recipes`, request);
  }

  /**
   * Remove a recipe from a collection
   */
  removeRecipeFromCollection(collectionId: string, recipeId: string): Observable<Collection> {
    return this.api.delete<Collection>(`${this.endpoint}/${collectionId}/recipes/${recipeId}`);
  }

  /**
   * Reorder recipes within a collection
   */
  reorderCollectionRecipes(collectionId: string, request: ReorderCollectionRecipesRequest): Observable<Collection> {
    return this.api.put<Collection>(`${this.endpoint}/${collectionId}/recipes/reorder`, request);
  }

  /**
   * Reorder user's collections
   */
  reorderCollections(request: ReorderCollectionsRequest): Observable<void> {
    return this.api.put<void>(`${this.endpoint}/reorder`, request);
  }
}
