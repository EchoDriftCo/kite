import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MealPlan } from '../../../models/meal-plan.model';

export interface CopyMealPlanResult {
  name: string;
  startDate: string;
  endDate: string;
}

@Component({
  selector: 'app-copy-meal-plan-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './copy-meal-plan-dialog.component.html',
  styleUrl: './copy-meal-plan-dialog.component.scss'
})
export class CopyMealPlanDialogComponent {
  name: string;
  startDate: Date;
  planDurationDays: number;

  constructor(
    private dialogRef: MatDialogRef<CopyMealPlanDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public plan: MealPlan
  ) {
    this.name = `Copy of ${plan.name}`;

    const origStart = new Date(plan.startDate);
    const origEnd = new Date(plan.endDate);
    this.planDurationDays = Math.round((origEnd.getTime() - origStart.getTime()) / (1000 * 60 * 60 * 24));

    // Default to next Monday
    const today = new Date();
    const nextMonday = new Date(today);
    nextMonday.setDate(today.getDate() + ((8 - today.getDay()) % 7 || 7));
    this.startDate = nextMonday;
  }

  get endDate(): Date {
    const end = new Date(this.startDate);
    end.setDate(end.getDate() + this.planDurationDays);
    return end;
  }

  get isValid(): boolean {
    return !!this.name.trim() && !!this.startDate;
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' });
  }

  confirm() {
    if (!this.isValid) return;

    const result: CopyMealPlanResult = {
      name: this.name.trim(),
      startDate: this.formatDateStr(this.startDate),
      endDate: this.formatDateStr(this.endDate)
    };

    this.dialogRef.close(result);
  }

  cancel() {
    this.dialogRef.close(null);
  }

  private formatDateStr(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
