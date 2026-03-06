// Models matching backend Circle API structure

export interface Circle {
  circleResourceId: string;
  name: string;
  description?: string;
  ownerSubjectId: string;
  isOwner?: boolean;
  memberCount?: number;
  recipeCount?: number;
  createdDate?: string;
}

export interface CircleMember {
  circleMemberResourceId?: string;
  circleId?: string;
  subjectId: string;
  userEmail?: string;
  role: CircleRole;
  status: MemberStatus;
  joinedDate?: string;
  invitedDate?: string;
}

export enum CircleRole {
  Owner = 'Owner',
  Admin = 'Admin',
  Member = 'Member'
}

export enum MemberStatus {
  Pending = 'Pending',
  Active = 'Active',
  Left = 'Left'
}

export interface CircleRecipe {
  circleRecipeResourceId?: string;
  circleId?: string;
  recipeResourceId: string;
  title?: string;
  recipeImageUrl?: string;
  sharedBySubjectId: string;
  sharedByEmail?: string;
  sharedDate: string;
}

export interface CircleInvite {
  circleInviteResourceId?: string;
  inviteToken: string;
  circleId?: string;
  circleName?: string;
  inviteeEmail?: string;
  invitedBySubjectId?: string;
  invitedByEmail?: string;
  createdDate?: string;
  expiresDate: string;
  status: InviteStatus;
}

export enum InviteStatus {
  Pending = 'Pending',
  Accepted = 'Accepted',
  Expired = 'Expired',
  Revoked = 'Revoked'
}

// Request/Response DTOs

export interface CreateCircleRequest {
  name: string;
  description?: string;
}

export interface UpdateCircleRequest {
  name?: string;
  description?: string;
}

export interface InviteMemberRequest {
  inviteeEmail?: string;
  expiresDate?: string;
}

export interface ShareRecipeRequest {
  recipeResourceId: string;
}

export interface AcceptInviteRequest {
  inviteToken: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
