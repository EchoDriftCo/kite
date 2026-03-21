import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { OnboardingService } from '../../services/onboarding.service';
import { TourService } from '../../services/tour.service';
import { AuthService } from '../../services/auth.service';
import { RecipeService } from '../../services/recipe.service';
import { OnboardingDialogComponent } from '../onboarding/onboarding-dialog/onboarding-dialog.component';
import { BetaInviteDialogComponent } from './beta-invite-dialog/beta-invite-dialog.component';

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
export class SettingsComponent implements OnInit {
  removingSamples = false;
  exporting = false;
  userEmail = '';
  appVersion = '1.0.0';

  constructor(
    private onboardingService: OnboardingService,
    private tourService: TourService,
    private authService: AuthService,
    private recipeService: RecipeService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.session$.subscribe(session => {
      this.userEmail = session?.user?.email || '';
    });
  }

  startTour(): void {
    this.tourService.forceStart();
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
            setTimeout(() => this.tourService.forceStart(), 500);
          }
        });
      },
      error: () => {
        this.snackBar.open('Failed to reset onboarding', 'OK', { duration: 3000 });
      }
    });
  }

  openBetaInviteDialog(): void {
    const dialogRef = this.dialog.open(BetaInviteDialogComponent, {
      width: '450px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe(redeemed => {
      if (redeemed) {
        this.snackBar.open('Beta invite code redeemed successfully!', 'OK', { duration: 5000 });
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

  changePassword(): void {
    // Navigate to Supabase password reset flow
    this.authService.resetPassword(this.userEmail).then(() => {
      this.snackBar.open('Password reset email sent. Check your inbox.', 'OK', { duration: 5000 });
    }).catch(() => {
      this.snackBar.open('Failed to send password reset email', 'OK', { duration: 3000 });
    });
  }

  async logout(): Promise<void> {
    try {
      await this.authService.signOut();
      this.router.navigate(['/login']);
    } catch {
      this.snackBar.open('Failed to sign out', 'OK', { duration: 3000 });
    }
  }

  exportRecipes(): void {
    this.exporting = true;
    // Fetch all recipes and download as JSON
    this.recipeService.searchRecipes({ pageSize: 1000, pageNumber: 1 }).subscribe({
      next: (data) => {
        const blob = new Blob([JSON.stringify(data.items || data, null, 2)], { type: 'application/json' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `recipevault-export-${new Date().toISOString().split('T')[0]}.json`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.exporting = false;
        this.snackBar.open('Recipes exported successfully!', 'OK', { duration: 3000 });
      },
      error: () => {
        this.exporting = false;
        this.snackBar.open('Failed to export recipes', 'OK', { duration: 3000 });
      }
    });
  }

  deleteAccount(): void {
    const confirmed = confirm(
      'Are you sure you want to delete your account? This will permanently delete all your recipes, meal plans, and data. This action CANNOT be undone.'
    );
    if (!confirmed) return;

    const doubleConfirm = prompt('Type DELETE to confirm account deletion:');
    if (doubleConfirm !== 'DELETE') {
      this.snackBar.open('Account deletion cancelled', 'OK', { duration: 3000 });
      return;
    }

    this.snackBar.open('Account deletion is not yet available. Contact support.', 'OK', { duration: 5000 });
  }
}
