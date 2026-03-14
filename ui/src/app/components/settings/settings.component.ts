import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { OnboardingService } from '../../services/onboarding.service';
import { TourService } from '../../services/tour.service';
import { OnboardingDialogComponent } from '../onboarding/onboarding-dialog/onboarding-dialog.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatDividerModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent {
  removingSamples = false;

  constructor(
    private onboardingService: OnboardingService,
    private tourService: TourService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  startTour(): void {
    this.tourService.start();
  }

  reRunOnboarding(): void {
    this.onboardingService.resetOnboarding().subscribe({
      next: () => {
        const dialogRef = this.dialog.open(OnboardingDialogComponent, {
          disableClose: false,
          width: '600px',
          maxWidth: '95vw',
          panelClass: 'onboarding-dialog'
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result?.startTour) {
            setTimeout(() => this.tourService.start(), 500);
          }
        });
      },
      error: () => {
        this.snackBar.open('Failed to reset onboarding', 'OK', { duration: 3000 });
      }
    });
  }

  removeSampleRecipes(): void {
    if (!confirm('This will delete all sample recipes from your library. Any edits you\'ve made will be lost.')) {
      return;
    }

    this.removingSamples = true;
    this.onboardingService.removeSampleRecipes().subscribe({
      next: result => {
        this.removingSamples = false;
        this.snackBar.open(
          `${result.recipesRemoved} sample recipes removed`,
          'OK',
          { duration: 3000 }
        );
      },
      error: () => {
        this.removingSamples = false;
        this.snackBar.open('Failed to remove sample recipes', 'OK', { duration: 3000 });
      }
    });
  }
}
