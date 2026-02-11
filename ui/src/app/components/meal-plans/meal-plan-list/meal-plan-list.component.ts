import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MealPlanService } from '../../../services/meal-plan.service';
import { MealPlan, MealPlanSearchRequest } from '../../../models/meal-plan.model';

@Component({
  selector: 'app-meal-plan-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatPaginatorModule
  ],
  templateUrl: './meal-plan-list.component.html',
  styleUrl: './meal-plan-list.component.scss'
})
export class MealPlanListComponent implements OnInit {
  mealPlans: MealPlan[] = [];
  loading = false;
  error = '';
  pageNumber = 1;
  pageSize = 12;
  totalCount = 0;

  constructor(
    private mealPlanService: MealPlanService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadMealPlans();
  }

  loadMealPlans() {
    this.loading = true;
    this.error = '';

    const request: MealPlanSearchRequest = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.mealPlanService.searchMealPlans(request).subscribe({
      next: (response) => {
        this.mealPlans = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load meal plans';
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadMealPlans();
  }

  viewMealPlan(plan: MealPlan) {
    this.router.navigate(['/meal-plans', plan.mealPlanResourceId]);
  }

  editMealPlan(plan: MealPlan, event: Event) {
    event.stopPropagation();
    this.router.navigate(['/meal-plans', plan.mealPlanResourceId, 'edit']);
  }

  createNew() {
    this.router.navigate(['/meal-plans/new']);
  }

  formatDateRange(plan: MealPlan): string {
    const start = new Date(plan.startDate);
    const end = new Date(plan.endDate);
    return `${start.toLocaleDateString()} - ${end.toLocaleDateString()}`;
  }
}
