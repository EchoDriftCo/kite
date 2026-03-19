import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CircleService } from '../../../services/circle.service';
import { Circle } from '../../../models/circle.model';
import { CircleFormDialogComponent } from '../circle-form-dialog/circle-form-dialog.component';

@Component({
  selector: 'app-circle-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './circle-list.component.html',
  styleUrl: './circle-list.component.scss'
})
export class CircleListComponent implements OnInit {
  circles: Circle[] = [];
  loading = false;
  error = '';

  constructor(
    private circleService: CircleService,
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
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

  openCreateDialog() {
    const dialogRef = this.dialog.open(CircleFormDialogComponent, {
      width: '500px',
      maxWidth: '95vw',
      data: { isEdit: false }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCircles();
        this.snackBar.open('Circle created successfully!', 'Close', { duration: 3000 });
      }
    });
  }

  viewCircle(circle: Circle) {
    this.router.navigate(['/circles', circle.circleResourceId]);
  }

  getMemberText(circle: Circle): string {
    const count = circle.memberCount || 0;
    return count === 1 ? '1 member' : `${count} members`;
  }

  getRecipeText(circle: Circle): string {
    const count = circle.recipeCount || 0;
    return count === 1 ? '1 recipe' : `${count} recipes`;
  }
}
