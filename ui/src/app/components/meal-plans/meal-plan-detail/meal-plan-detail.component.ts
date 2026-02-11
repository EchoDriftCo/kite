import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MealPlanService } from '../../../services/meal-plan.service';
import { MealPlan, MealPlanEntry, MealSlot, MEAL_SLOT_LABELS, MealPlanEntryRequest, CreateMealPlanRequest } from '../../../models/meal-plan.model';
import { RecipePickerDialogComponent } from '../recipe-picker-dialog/recipe-picker-dialog.component';

interface CalendarDay {
  date: Date;
  dateStr: string;
  dayLabel: string;
  entries: MealPlanEntry[];
}

@Component({
  selector: 'app-meal-plan-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatCheckboxModule,
    MatChipsModule
  ],
  templateUrl: './meal-plan-detail.component.html',
  styleUrl: './meal-plan-detail.component.scss'
})
export class MealPlanDetailComponent implements OnInit {
  plan: MealPlan | null = null;
  loading = false;
  error = '';
  planId: string | null = null;

  calendarDays: CalendarDay[] = [];
  mealSlots = [MealSlot.Breakfast, MealSlot.Lunch, MealSlot.Dinner, MealSlot.Snack];
  mealSlotLabels = MEAL_SLOT_LABELS;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private mealPlanService: MealPlanService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.planId = this.route.snapshot.paramMap.get('id');
    if (this.planId) {
      this.loadPlan();
    }
  }

  loadPlan() {
    if (!this.planId) return;

    this.loading = true;
    this.error = '';

    this.mealPlanService.getMealPlan(this.planId).subscribe({
      next: (plan) => {
        this.plan = plan;
        this.buildCalendar();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load meal plan';
        this.loading = false;
      }
    });
  }

  buildCalendar() {
    if (!this.plan) return;

    this.calendarDays = [];
    const start = new Date(this.plan.startDate);
    const end = new Date(this.plan.endDate);

    const current = new Date(start);
    while (current <= end) {
      const dateStr = this.formatDateStr(current);
      const dayEntries = (this.plan.entries || []).filter(e => {
        const entryDate = this.formatDateStr(new Date(e.date));
        return entryDate === dateStr;
      });

      this.calendarDays.push({
        date: new Date(current),
        dateStr,
        dayLabel: current.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' }),
        entries: dayEntries
      });

      current.setDate(current.getDate() + 1);
    }
  }

  getEntriesForSlot(day: CalendarDay, slot: MealSlot): MealPlanEntry[] {
    return day.entries.filter(e => e.mealSlot === slot);
  }

  addEntry(day: CalendarDay, slot: MealSlot) {
    const dialogRef = this.dialog.open(RecipePickerDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: { date: day.dateStr, mealSlot: slot }
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && this.plan) {
        const newEntry: MealPlanEntryRequest = {
          date: day.dateStr,
          mealSlot: slot,
          recipeResourceId: result.recipeResourceId,
          servings: result.servings || undefined,
          isLeftover: false
        };

        // Build full entries list for update
        const existingEntries: MealPlanEntryRequest[] = (this.plan.entries || []).map(e => ({
          date: this.formatDateStr(new Date(e.date)),
          mealSlot: e.mealSlot,
          recipeResourceId: e.recipeResourceId,
          servings: e.servings || undefined,
          isLeftover: e.isLeftover
        }));
        existingEntries.push(newEntry);

        this.savePlanEntries(existingEntries);
      }
    });
  }

  removeEntry(entry: MealPlanEntry) {
    if (!this.plan) return;

    const updatedEntries: MealPlanEntryRequest[] = (this.plan.entries || [])
      .filter(e => e !== entry)
      .map(e => ({
        date: this.formatDateStr(new Date(e.date)),
        mealSlot: e.mealSlot,
        recipeResourceId: e.recipeResourceId,
        servings: e.servings || undefined,
        isLeftover: e.isLeftover
      }));

    this.savePlanEntries(updatedEntries);
  }

  toggleLeftover(entry: MealPlanEntry) {
    if (!this.plan) return;

    const updatedEntries: MealPlanEntryRequest[] = (this.plan.entries || []).map(e => ({
      date: this.formatDateStr(new Date(e.date)),
      mealSlot: e.mealSlot,
      recipeResourceId: e.recipeResourceId,
      servings: e.servings || undefined,
      isLeftover: e === entry ? !e.isLeftover : e.isLeftover
    }));

    this.savePlanEntries(updatedEntries);
  }

  private savePlanEntries(entries: MealPlanEntryRequest[]) {
    if (!this.plan || !this.planId) return;

    const request: CreateMealPlanRequest = {
      name: this.plan.name,
      startDate: this.formatDateStr(new Date(this.plan.startDate)),
      endDate: this.formatDateStr(new Date(this.plan.endDate)),
      entries
    };

    this.mealPlanService.updateMealPlan(this.planId, request).subscribe({
      next: (updatedPlan) => {
        this.plan = updatedPlan;
        this.buildCalendar();
      },
      error: (err) => {
        this.error = err.message || 'Failed to update meal plan';
      }
    });
  }

  editPlan() {
    if (this.planId) {
      this.router.navigate(['/meal-plans', this.planId, 'edit']);
    }
  }

  deletePlan() {
    if (!this.planId || !confirm('Are you sure you want to delete this meal plan?')) return;

    this.mealPlanService.deleteMealPlan(this.planId).subscribe({
      next: () => this.router.navigate(['/meal-plans']),
      error: (err) => {
        this.error = err.message || 'Failed to delete meal plan';
      }
    });
  }

  viewGroceryList() {
    if (this.planId) {
      this.router.navigate(['/meal-plans', this.planId, 'grocery-list']);
    }
  }

  goBack() {
    this.router.navigate(['/meal-plans']);
  }

  private formatDateStr(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
