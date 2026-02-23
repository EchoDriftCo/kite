import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CircleService } from '../../../services/circle.service';
import { Circle } from '../../../models/circle.model';
import { forkJoin } from 'rxjs';

export interface ShareRecipeDialogData {
  recipeId: string;
  recipeTitle: string;
}

@Component({
  selector: 'app-share-recipe-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './share-recipe-dialog.component.html',
  styleUrl: './share-recipe-dialog.component.scss'
})
export class ShareRecipeDialogComponent implements OnInit {
  circles: Circle[] = [];
  selectedCircleIds: Set<string> = new Set();
  loading = false;
  saving = false;
  error = '';

  constructor(
    private circleService: CircleService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ShareRecipeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ShareRecipeDialogData
  ) {}

  ngOnInit() {
    this.loadCircles();
  }

  loadCircles() {
    this.loading = true;
    this.error = '';

    this.circleService.getCircles().subscribe({
      next: (response) => {
        this.circles = response.items || [];
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load circles';
        this.loading = false;
        console.error('Error loading circles:', err);
      }
    });
  }

  toggleCircle(circleId: string) {
    if (this.selectedCircleIds.has(circleId)) {
      this.selectedCircleIds.delete(circleId);
    } else {
      this.selectedCircleIds.add(circleId);
    }
  }

  isSelected(circleId: string): boolean {
    return this.selectedCircleIds.has(circleId);
  }

  onShare() {
    if (this.selectedCircleIds.size === 0) {
      return;
    }

    this.saving = true;
    this.error = '';

    // Share to all selected circles
    const shareRequests = Array.from(this.selectedCircleIds).map(circleId =>
      this.circleService.shareRecipe(circleId, { recipeResourceId: this.data.recipeId })
    );

    forkJoin(shareRequests).subscribe({
      next: () => {
        this.saving = false;
        const count = this.selectedCircleIds.size;
        const message = count === 1 ? 'Shared to 1 circle' : `Shared to ${count} circles`;
        this.snackBar.open(message, 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.error = err.message || 'Failed to share recipe';
        this.saving = false;
        console.error('Error sharing recipe:', err);
      }
    });
  }

  onCancel() {
    this.dialogRef.close();
  }
}
