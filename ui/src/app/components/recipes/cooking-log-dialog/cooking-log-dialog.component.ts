import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CookingLogService } from '../../../services/cooking-log.service';
import { CreateCookingLogRequest } from '../../../models/cooking-log.model';

export interface CookingLogDialogData {
  recipeId: string;
  recipeTitle: string;
  recipeYield: number;
  lastNote?: string;
}

export interface CookingLogDialogResult {
  success: boolean;
}

@Component({
  selector: 'app-cooking-log-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './cooking-log-dialog.component.html',
  styleUrl: './cooking-log-dialog.component.scss'
})
export class CookingLogDialogComponent implements OnInit {
  cookedDate: Date = new Date();
  servingsMade?: number;
  rating?: number;
  notes = '';
  selectedPhoto?: string; // base64
  photoMimeType?: string;
  
  loading = false;
  error = '';

  hoverRating = 0;

  constructor(
    public dialogRef: MatDialogRef<CookingLogDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CookingLogDialogData,
    private cookingLogService: CookingLogService
  ) {}

  ngOnInit() {
    // Default servings to recipe yield
    this.servingsMade = this.data.recipeYield;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (!this.cookedDate) {
      this.error = 'Please select a date';
      return;
    }

    this.loading = true;
    this.error = '';

    const request: CreateCookingLogRequest = {
      recipeId: this.data.recipeId,
      cookedDate: this.cookedDate.toISOString(),
      servingsMade: this.servingsMade,
      rating: this.rating,
      notes: this.notes || undefined
    };

    this.cookingLogService.createLog(request).subscribe({
      next: (entry) => {
        // If photo selected, upload it
        if (this.selectedPhoto && this.photoMimeType) {
          this.cookingLogService.addPhoto(entry.cookingLogId, this.selectedPhoto, this.photoMimeType).subscribe({
            next: () => {
              this.loading = false;
              this.dialogRef.close({ success: true });
            },
            error: (err) => {
              // Log created but photo failed - still close dialog
              console.error('Failed to upload photo:', err);
              this.loading = false;
              this.dialogRef.close({ success: true });
            }
          });
        } else {
          this.loading = false;
          this.dialogRef.close({ success: true });
        }
      },
      error: (err) => {
        this.error = err.message || 'Failed to log cooking session';
        this.loading = false;
        console.error('Error creating cooking log:', err);
      }
    });
  }

  setRating(stars: number): void {
    // Click same star to clear rating
    this.rating = this.rating === stars ? undefined : stars;
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.photoMimeType = file.type;

      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        // Extract base64 data (remove data:image/...;base64, prefix)
        this.selectedPhoto = result.split(',')[1];
      };
      reader.readAsDataURL(file);
    }
  }

  removePhoto(): void {
    this.selectedPhoto = undefined;
    this.photoMimeType = undefined;
  }

  getPhotoDataUrl(): string {
    if (!this.selectedPhoto || !this.photoMimeType) return '';
    return `data:${this.photoMimeType};base64,${this.selectedPhoto}`;
  }
}
