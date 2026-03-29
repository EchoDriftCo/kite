import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { UserAccountService } from '../../../services/user-account.service';
import { BetaInviteDialogComponent } from '../../../components/settings/beta-invite-dialog/beta-invite-dialog.component';

@Component({
  selector: 'app-upgrade-banner',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  template: `
    @if (showBanner) {
      <div class="upgrade-banner">
        <mat-icon>star</mat-icon>
        <span class="banner-text">
          You're on the <strong>Free</strong> tier.
          Have a beta invite code? Unlock more features!
        </span>
        <button mat-stroked-button (click)="openInviteDialog()">
          <mat-icon>vpn_key</mat-icon>
          Enter Code
        </button>
        <button mat-icon-button class="dismiss-btn" (click)="dismiss()" aria-label="Dismiss banner">
          <mat-icon>close</mat-icon>
        </button>
      </div>
    }
  `,
  styles: [`
    .upgrade-banner {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px 16px;
      background: linear-gradient(135deg, rgba(245, 158, 11, 0.15), rgba(124, 152, 133, 0.1));
      border-bottom: 1px solid rgba(245, 158, 11, 0.3);

      > mat-icon {
        color: #f59e0b;
        flex-shrink: 0;
      }
    }

    .banner-text {
      flex: 1;
      font-size: 13px;
      color: var(--color-text-primary);

      strong {
        color: #f59e0b;
      }
    }

    button[mat-stroked-button] {
      flex-shrink: 0;
      border-color: #f59e0b;
      color: #f59e0b;
      font-size: 12px;
    }

    .dismiss-btn {
      flex-shrink: 0;
      color: var(--color-text-hint);
      width: 28px;
      height: 28px;
      line-height: 28px;

      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
      }
    }

    @media (max-width: 600px) {
      .upgrade-banner {
        flex-wrap: wrap;
        gap: 8px;
      }

      .banner-text {
        flex-basis: calc(100% - 48px);
      }
    }
  `]
})
export class UpgradeBannerComponent implements OnInit, OnDestroy {
  showBanner = false;
  private dismissed = false;
  private sub?: Subscription;

  constructor(
    private authService: AuthService,
    private userAccountService: UserAccountService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.dismissed = sessionStorage.getItem('upgrade-banner-dismissed') === 'true';
    this.sub = this.authService.currentUser$.subscribe(() => {
      if (!this.authService.isAuthenticated() || this.dismissed) {
        this.showBanner = false;
        return;
      }

      // Fetch server-side account state to check beta code redemption and tier
      this.userAccountService.getAccount().subscribe({
        next: (account) => {
          this.showBanner = !account.betaCodeRedeemedDate
            && account.accountTier === 'Free';
        },
        error: () => {
          // Fallback to JWT tier if backend is unreachable
          this.showBanner = this.authService.getUserTier() === 'Free';
        }
      });
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  dismiss(): void {
    this.dismissed = true;
    this.showBanner = false;
    sessionStorage.setItem('upgrade-banner-dismissed', 'true');
  }

  openInviteDialog(): void {
    const dialogRef = this.dialog.open(BetaInviteDialogComponent, {
      width: '450px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe(redeemed => {
      if (redeemed) {
        this.showBanner = false;
        this.snackBar.open('Beta invite code redeemed successfully!', 'OK', { duration: 5000 });
      }
    });
  }
}
