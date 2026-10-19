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
import { GuestPromptService } from '../../services/guest-prompt.service';
import { RecipeService } from '../../services/recipe.service';
import { ConfirmService } from '../../services/confirm.service';
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
  isGuest = false;
  appVersion = '1.0.0';

  constructor(
    private onboardingService: OnboardingService,
    private tourService: TourService,
    private authService: AuthService,
    private recipeService: RecipeService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router,
    private guestPrompt: GuestPromptService,
    private confirm: ConfirmService
  ) {}

  ngOnInit(): void {
    this.authService.session$.subscribe(session => {
      this.userEmail = session?.user?.email || '';
      this.isGuest = !!session?.user?.is_anonymous;
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

  async removeSampleRecipes(): Promise<void> {
    if (!(await this.confirm.ask({
      title: 'Remove sample recipes?',
      message: "All sample recipes will be deleted from your library, including any edits you've made to them.",
      confirmText: 'Remove',
      destructive: true,
    }))) {
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

  createAccount(): void {
    this.guestPrompt.promptSignUp();
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
    if (this.isGuest && !(await this.confirm.ask({
      title: 'Sign out of guest session?',
      message: 'Signing out of a guest session discards your recipes and data. Create an account first to keep them.',
      confirmText: 'Sign out',
      destructive: true,
    }))) {
      return;
    }
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

  async deleteAccount(): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'Delete your account?',
      message: "All your recipes, meal plans and data will be permanently deleted. This can't be undone.",
      confirmText: 'Delete account',
      destructive: true,
      requireText: 'DELETE',
    });
    if (!confirmed) return;

    this.snackBar.open('Account deletion is not yet available. Contact support.', 'OK', { duration: 5000 });
  }
}
