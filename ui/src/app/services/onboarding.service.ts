import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  OnboardingStatus,
  OnboardingProgress,
  AddSampleRecipesResult,
  RemoveSampleRecipesResult
} from '../models/onboarding.model';

@Injectable({
  providedIn: 'root'
})
export class OnboardingService {
  constructor(private api: ApiService) { }

  getStatus(): Observable<OnboardingStatus> {
    return this.api.get<OnboardingStatus>('user/onboarding-status');
  }

  updateProgress(progress: Partial<OnboardingProgress>): Observable<void> {
    return this.api.patch<void>('user/onboarding-progress', progress);
  }

  completeOnboarding(): Observable<void> {
    return this.api.post<void>('user/complete-onboarding', {});
  }

  resetOnboarding(): Observable<void> {
    return this.api.post<void>('user/reset-onboarding', {});
  }

  addSampleRecipes(): Observable<AddSampleRecipesResult> {
    return this.api.post<AddSampleRecipesResult>('user/add-sample-recipes', {});
  }

  removeSampleRecipes(): Observable<RemoveSampleRecipesResult> {
    return this.api.delete<RemoveSampleRecipesResult>('user/sample-recipes');
  }
}
