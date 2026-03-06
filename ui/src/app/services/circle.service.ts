import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Circle,
  CircleMember,
  CircleRecipe,
  CircleInvite,
  CreateCircleRequest,
  UpdateCircleRequest,
  InviteMemberRequest,
  ShareRecipeRequest,
  AcceptInviteRequest,
  PagedResult
} from '../models/circle.model';

@Injectable({
  providedIn: 'root'
})
export class CircleService {
  private readonly endpoint = 'circles';

  constructor(private api: ApiService) {}

  // Circle CRUD operations

  /**
   * Create a new circle
   */
  createCircle(request: CreateCircleRequest): Observable<Circle> {
    return this.api.post<Circle>(this.endpoint, request);
  }

  /**
   * Get all circles (owned + member of)
   */
  getCircles(pageNumber = 1, pageSize = 50): Observable<PagedResult<Circle>> {
    const params = `pageNumber=${pageNumber}&pageSize=${pageSize}`;
    return this.api.get<PagedResult<Circle>>(`${this.endpoint}?${params}`);
  }

  /**
   * Get a single circle by ID
   */
  getCircle(circleId: string): Observable<Circle> {
    return this.api.get<Circle>(`${this.endpoint}/${circleId}`);
  }

  /**
   * Update circle details (owner only)
   */
  updateCircle(circleId: string, request: UpdateCircleRequest): Observable<Circle> {
    return this.api.put<Circle>(`${this.endpoint}/${circleId}`, request);
  }

  /**
   * Delete circle (owner only)
   */
  deleteCircle(circleId: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${circleId}`);
  }

  // Member management

  /**
   * Get all members of a circle
   */
  getCircleMembers(circleId: string, pageNumber = 1, pageSize = 100): Observable<PagedResult<CircleMember>> {
    const params = `pageNumber=${pageNumber}&pageSize=${pageSize}`;
    return this.api.get<PagedResult<CircleMember>>(`${this.endpoint}/${circleId}/members?${params}`);
  }

  /**
   * Invite a member by email or generate shareable link
   */
  inviteMember(circleId: string, request: InviteMemberRequest): Observable<CircleInvite> {
    return this.api.post<CircleInvite>(`${this.endpoint}/${circleId}/invite`, request);
  }

  /**
   * Remove a member from a circle
   */
  removeMember(circleId: string, subjectId: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${circleId}/members/${subjectId}`);
  }

  /**
   * Leave a circle as the current user
   */
  leaveCircle(circleId: string): Observable<void> {
    return this.api.post<void>(`${this.endpoint}/${circleId}/leave`, {});
  }

  /**
   * Change member role (owner only)
   */
  updateMemberRole(circleId: string, subjectId: string, role: string): Observable<CircleMember> {
    return this.api.put<CircleMember>(`${this.endpoint}/${circleId}/members/${subjectId}`, { role });
  }

  // Invite management

  /**
   * Get invite details (for preview before accepting)
   */
  getInviteDetails(inviteToken: string): Observable<CircleInvite> {
    return this.api.get<CircleInvite>(`${this.endpoint}/invite/${inviteToken}`);
  }

  /**
   * Accept an invite
   */
  acceptInvite(request: AcceptInviteRequest): Observable<Circle> {
    return this.api.post<Circle>(`${this.endpoint}/join/${request.inviteToken}`, {});
  }

  // Recipe sharing

  /**
   * Share a recipe to a circle
   */
  shareRecipe(circleId: string, request: ShareRecipeRequest): Observable<CircleRecipe> {
    return this.api.post<CircleRecipe>(`${this.endpoint}/${circleId}/recipes`, request);
  }

  /**
   * Unshare a recipe from a circle
   */
  unshareRecipe(circleId: string, recipeId: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${circleId}/recipes/${recipeId}`);
  }

  /**
   * Get all recipes shared to a circle
   */
  getCircleRecipes(circleId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<CircleRecipe>> {
    const params = `pageNumber=${pageNumber}&pageSize=${pageSize}`;
    return this.api.get<PagedResult<CircleRecipe>>(`${this.endpoint}/${circleId}/recipes?${params}`);
  }

  /**
   * Get circles that a specific recipe is shared to
   */
  getRecipeCircles(recipeId: string): Observable<Circle[]> {
    return this.api.get<Circle[]>(`recipes/${recipeId}/circles`);
  }
}
