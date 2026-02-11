import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MealPlanService } from '../../../services/meal-plan.service';
import { MealPlan, CreateMealPlanRequest } from '../../../models/meal-plan.model';

@Component({
  selector: 'app-meal-plan-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './meal-plan-form.component.html',
  styleUrl: './meal-plan-form.component.scss'
})
export class MealPlanFormComponent implements OnInit {
  planForm!: FormGroup;
  loading = false;
  saving = false;
  error = '';
  planId: string | null = null;
  isEditMode = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private mealPlanService: MealPlanService
  ) {
    this.initForm();
  }

  ngOnInit() {
    this.planId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.planId;

    if (this.isEditMode && this.planId) {
      this.loadPlan();
    } else {
      // Default to starting next Monday for a week
      const today = new Date();
      const nextMonday = new Date(today);
      nextMonday.setDate(today.getDate() + ((8 - today.getDay()) % 7 || 7));
      const nextSunday = new Date(nextMonday);
      nextSunday.setDate(nextMonday.getDate() + 6);

      this.planForm.patchValue({
        startDate: nextMonday,
        endDate: nextSunday
      });
    }
  }

  initForm() {
    this.planForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      startDate: [null, Validators.required],
      endDate: [null, Validators.required]
    });
  }

  loadPlan() {
    if (!this.planId) return;

    this.loading = true;
    this.error = '';

    this.mealPlanService.getMealPlan(this.planId).subscribe({
      next: (plan) => {
        this.planForm.patchValue({
          name: plan.name,
          startDate: new Date(plan.startDate),
          endDate: new Date(plan.endDate)
        });
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load meal plan';
        this.loading = false;
      }
    });
  }

  onSubmit() {
    if (this.planForm.invalid) {
      this.planForm.markAllAsTouched();
      this.error = 'Please fix the errors in the form';
      return;
    }

    const formValue = this.planForm.value;
    const request: CreateMealPlanRequest = {
      name: formValue.name,
      startDate: this.formatDate(formValue.startDate),
      endDate: this.formatDate(formValue.endDate)
    };

    this.saving = true;
    this.error = '';

    const operation = this.isEditMode && this.planId
      ? this.mealPlanService.updateMealPlan(this.planId, request)
      : this.mealPlanService.createMealPlan(request);

    operation.subscribe({
      next: (plan) => {
        this.saving = false;
        this.router.navigate(['/meal-plans', plan.mealPlanResourceId]);
      },
      error: (err) => {
        this.error = err.message || 'Failed to save meal plan';
        this.saving = false;
      }
    });
  }

  cancel() {
    if (this.isEditMode && this.planId) {
      this.router.navigate(['/meal-plans', this.planId]);
    } else {
      this.router.navigate(['/meal-plans']);
    }
  }

  private formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
