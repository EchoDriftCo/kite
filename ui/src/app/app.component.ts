import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subscription, distinctUntilChanged, filter, map } from 'rxjs';
import { AuthService } from './services/auth.service';
import { GuestPromptService } from './services/guest-prompt.service';
import { OnboardingService } from './services/onboarding.service';
import { TourService } from './services/tour.service';
import { ThemeService } from './services/theme.service';
import { FeedbackButtonComponent } from './shared/components/feedback-button/feedback-button.component';
import { InstallPromptComponent } from './shared/components/install-prompt/install-prompt.component';
import { OnboardingDialogComponent } from './components/onboarding/onboarding-dialog/onboarding-dialog.component';
import { TourTooltipComponent } from './components/onboarding/tour-tooltip/tour-tooltip.component';
import { UpgradeBannerComponent } from './shared/components/upgrade-banner/upgrade-banner.component';
import { ConfirmService } from './services/confirm.service';

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
export class AppComponent implements OnInit, OnDestroy {
  private userSub?: Subscription;

  constructor(
    private authService: AuthService,
    private dialog: MatDialog,
    private onboardingService: OnboardingService,
    private tourService: TourService,
    public themeService: ThemeService,
    private guestPrompt: GuestPromptService,
    private router: Router,
    private confirm: ConfirmService
  ) {}

  ngOnInit(): void {
    // Check onboarding whenever someone signs in, including guests whose session starts in the auth guard
    this.userSub = this.authService.currentUser$.pipe(
      map(user => user?.id),
      distinctUntilChanged(),
      filter(id => !!id)
    ).subscribe(() => this.checkOnboarding());
  }

  ngOnDestroy(): void {
    this.userSub?.unsubscribe();
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  isGuest(): boolean {
    return this.authService.isGuest();
  }

  userEmail(): string {
    if (this.isGuest()) {
      return 'Guest';
    }
    return this.authService.currentUser?.email || 'User';
  }

  createAccount(): void {
    this.guestPrompt.promptSignUp();
  }

  signIn(): void {
    this.router.navigate(['/login']);
  }

  async logout(): Promise<void> {
    if (this.isGuest() && !(await this.confirm.ask({
      title: 'Sign out of guest session?',
      message: 'Signing out of a guest session discards your recipes and data. Create an account first to keep them.',
      confirmText: 'Sign out',
      destructive: true,
    }))) {
      return;
    }
    this.authService.signOut();
  }

  toggleTheme(): void {
    this.themeService.toggle();
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
