import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  MealPlan,
  CreateMealPlanRequest,
  MealPlanSearchRequest,
  GroceryList
} from '../models/meal-plan.model';
import { PagedResult } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class MealPlanService {
  private readonly endpoint = 'meal-plans';

  constructor(private api: ApiService) {}

  searchMealPlans(request: MealPlanSearchRequest = {}): Observable<PagedResult<MealPlan>> {
    const params: any = {};

    if (request.pageNumber) params.pageNumber = request.pageNumber;
    if (request.pageSize) params.pageSize = request.pageSize || 20;
    if (request.startDateFrom) params.startDateFrom = request.startDateFrom;
    if (request.startDateTo) params.startDateTo = request.startDateTo;

    const queryString = Object.keys(params)
      .map(key => `${key}=${encodeURIComponent(params[key])}`)
      .join('&');

    const url = queryString ? `${this.endpoint}?${queryString}` : this.endpoint;

    return this.api.get<PagedResult<MealPlan>>(url);
  }

  getMealPlan(id: string): Observable<MealPlan> {
    return this.api.get<MealPlan>(`${this.endpoint}/${id}`);
  }

  createMealPlan(mealPlan: CreateMealPlanRequest): Observable<MealPlan> {
    return this.api.post<MealPlan>(this.endpoint, mealPlan);
  }

  updateMealPlan(id: string, mealPlan: CreateMealPlanRequest): Observable<MealPlan> {
    return this.api.put<MealPlan>(`${this.endpoint}/${id}`, mealPlan);
  }

  deleteMealPlan(id: string): Observable<void> {
    return this.api.delete<void>(`${this.endpoint}/${id}`);
  }

  getGroceryList(id: string): Observable<GroceryList> {
    return this.api.get<GroceryList>(`${this.endpoint}/${id}/grocery-list`);
  }
}
