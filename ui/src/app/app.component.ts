import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AuthService } from './services/auth.service';
import { OnboardingService } from './services/onboarding.service';
import { TourService } from './services/tour.service';
import { FeedbackButtonComponent } from './shared/components/feedback-button/feedback-button.component';
import { InstallPromptComponent } from './shared/components/install-prompt/install-prompt.component';
import { OnboardingDialogComponent } from './components/onboarding/onboarding-dialog/onboarding-dialog.component';
import { TourTooltipComponent } from './components/onboarding/tour-tooltip/tour-tooltip.component';
import { UpgradeBannerComponent } from './shared/components/upgrade-banner/upgrade-banner.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule,
    MatDialogModule,
    FeedbackButtonComponent,
    InstallPromptComponent,
    TourTooltipComponent,
    UpgradeBannerComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private dialog: MatDialog,
    private onboardingService: OnboardingService,
    private tourService: TourService
  ) {}

  ngOnInit(): void {
    this.authService.ready.then(() => {
      if (this.authService.isAuthenticated()) {
        this.checkOnboarding();
      }
    });
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  userEmail(): string {
    return this.authService.currentUser?.email || 'User';
  }

  logout(): void {
    this.authService.signOut();
  }

  private checkOnboarding(): void {
    this.onboardingService.getStatus().subscribe({
      next: status => {
        if (!status.hasCompletedOnboarding) {
          this.showOnboardingDialog();
        }
      },
      error: () => {
        // Silently fail — don't block the app if onboarding check fails
      }
    });
  }

  private showOnboardingDialog(): void {
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
  }
}
