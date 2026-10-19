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
import { CopyMealPlanDialogComponent, CopyMealPlanResult } from '../copy-meal-plan-dialog/copy-meal-plan-dialog.component';
import { addDays, daysBetween, formatCalendarDate, parseCalendarDate } from '../../../shared/calendar-date';
import { ConfirmService } from '../../../services/confirm.service';

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
    private dialog: MatDialog,
    private confirm: ConfirmService
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
    const start = parseCalendarDate(this.plan.startDate);
    const end = parseCalendarDate(this.plan.endDate);

    const current = new Date(start);
    while (current <= end) {
      const dateStr = formatCalendarDate(current);
      const dayEntries = (this.plan.entries || []).filter(e => {
        const entryDate = formatCalendarDate(parseCalendarDate(e.date));
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
          date: formatCalendarDate(parseCalendarDate(e.date)),
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
        date: formatCalendarDate(parseCalendarDate(e.date)),
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
      date: formatCalendarDate(parseCalendarDate(e.date)),
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
      startDate: formatCalendarDate(parseCalendarDate(this.plan.startDate)),
      endDate: formatCalendarDate(parseCalendarDate(this.plan.endDate)),
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

  copyPlan() {
    if (!this.plan) return;

    const dialogRef = this.dialog.open(CopyMealPlanDialogComponent, {
      width: '450px',
      maxWidth: '90vw',
      data: this.plan
    });

    dialogRef.afterClosed().subscribe((result: CopyMealPlanResult | null) => {
      if (result && this.plan) {
        const offsetDays = daysBetween(parseCalendarDate(this.plan.startDate), parseCalendarDate(result.startDate));

        const shiftedEntries: MealPlanEntryRequest[] = (this.plan.entries || []).map(e => {
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
  }

  editPlan() {
    if (this.planId) {
      this.router.navigate(['/meal-plans', this.planId, 'edit']);
    }
  }

  async deletePlan() {
    if (!this.planId || !(await this.confirm.ask({
      title: 'Delete meal plan?',
      message: `"${this.plan?.name ?? 'This meal plan'}" will be deleted. Your recipes aren't affected.`,
      confirmText: 'Delete',
      destructive: true,
    }))) return;

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
}
