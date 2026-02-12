import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Tag,
  CreateTagRequest,
  UpdateTagRequest,
  TagSearchRequest
} from '../models/tag.model';
import { PagedResult } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class TagService {
  private readonly endpoint = 'tags';

  constructor(private api: ApiService) {}

  /**
   * Search/list tags with pagination
   */
  searchTags(request: TagSearchRequest = {}): Observable<PagedResult<Tag>> {
    const params: any = {};

    if (request.pageNumber) params.pageNumber = request.pageNumber;
    if (request.pageSize) params.pageSize = request.pageSize || 50;
    if (request.sortBy) params.sortBy = request.sortBy;
    if (request.sortDirection) params.sortDirection = request.sortDirection;
    if (request.name) params.name = request.name;
    if (request.category != null) params.category = request.category;
    if (request.isGlobal != null) params.isGlobal = request.isGlobal;

    // Build query string
    const queryString = Object.keys(params)
      .map(key => `${key}=${encodeURIComponent(params[key])}`)
      .join('&');

    const url = queryString ? `${this.endpoint}?${queryString}` : this.endpoint;

    return this.api.get<PagedResult<Tag>>(url);
  }

  /**
   * Get a single tag by ID
   */
  getTag(id: string): Observable<Tag> {
    return this.api.get<Tag>(`${this.endpoint}/${id}`);
  }

  /**
   * Create a new tag
   */
  createTag(request: CreateTagRequest): Observable<Tag> {
    return this.api.post<Tag>(this.endpoint, request);
  }

  /**
   * Update an existing tag
   */
  updateTag(id: string, request: UpdateTagRequest): Observable<Tag> {
    return this.api.put<Tag>(`${this.endpoint}/${id}`, request);
  }

  /**
   * Delete a tag
   */
  deleteTag(id: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${id}`);
  }
}
