import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { MealPlanService } from '../../../services/meal-plan.service';
import { MealPlan, MealPlanSearchRequest, MealPlanEntryRequest, CreateMealPlanRequest } from '../../../models/meal-plan.model';
import { CopyMealPlanDialogComponent, CopyMealPlanResult } from '../copy-meal-plan-dialog/copy-meal-plan-dialog.component';
import { addDays, daysBetween, formatCalendarDate, parseCalendarDate } from '../../../shared/calendar-date';

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
    private router: Router,
    private dialog: MatDialog
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

  copyMealPlan(plan: MealPlan, event: Event) {
    event.stopPropagation();

    // Fetch full plan data (list may not include entries)
    this.mealPlanService.getMealPlan(plan.mealPlanResourceId).subscribe({
      next: (fullPlan) => {
        const dialogRef = this.dialog.open(CopyMealPlanDialogComponent, {
          width: '450px',
          maxWidth: '90vw',
          data: fullPlan
        });

        dialogRef.afterClosed().subscribe((result: CopyMealPlanResult | null) => {
          if (result) {
            const offsetDays = daysBetween(parseCalendarDate(fullPlan.startDate), parseCalendarDate(result.startDate));

            const shiftedEntries: MealPlanEntryRequest[] = (fullPlan.entries || []).map(e => {
              const shiftedDate = addDays(parseCalendarDate(e.date), offsetDays);
              return {
                date: formatCalendarDate(shiftedDate),
                mealSlot: e.mealSlot,
                recipeResourceId: e.recipeResourceId,
                servings: e.servings || undefined,
                isLeftover: e.isLeftover
              };
            });

            const request: CreateMealPlanRequest = {
              name: result.name,
              startDate: result.startDate,
              endDate: result.endDate,
              entries: shiftedEntries
            };

            this.mealPlanService.createMealPlan(request).subscribe({
              next: (newPlan) => {
                this.router.navigate(['/meal-plans', newPlan.mealPlanResourceId]);
              },
              error: (err) => {
                this.error = err.message || 'Failed to copy meal plan';
              }
            });
          }
        });
      },
      error: (err) => {
        this.error = err.message || 'Failed to load meal plan for copying';
      }
    });
  }

  createNew() {
    this.router.navigate(['/meal-plans/new']);
  }

  formatDateRange(plan: MealPlan): string {
    const start = parseCalendarDate(plan.startDate);
    const end = parseCalendarDate(plan.endDate);
    return `${start.toLocaleDateString()} - ${end.toLocaleDateString()}`;
  }
}
