import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="reset-page">
      <div class="reset-container">
        <h1 class="logo">
          <mat-icon class="logo-icon">restaurant_menu</mat-icon>
          RecipeVault
        </h1>

        @if (!emailSent) {
          <mat-card class="reset-card">
            <mat-card-header>
              <mat-card-title>Reset Password</mat-card-title>
            </mat-card-header>

            <mat-card-content>
              <p class="instructions">
                Enter your email address and we'll send you a link to reset your password.
              </p>

              <form (ngSubmit)="sendResetLink()">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Email</mat-label>
                  <input matInput type="email" [(ngModel)]="email" name="email"
                         autocomplete="email" placeholder="you@example.com" required>
                </mat-form-field>

                <button mat-raised-button color="primary" type="submit"
                        class="full-width-button" [disabled]="loading || !email">
                  @if (loading) {
                    <mat-spinner diameter="20"></mat-spinner>
                    Sending...
                  } @else {
                    SEND RESET LINK
                  }
                </button>

                @if (error) {
                  <div class="error-message" role="alert">
                    <mat-icon>error</mat-icon>
                    <span>{{ error }}</span>
                  </div>
                }
              </form>

              <p class="signin-prompt">
                Remember your password?
                <a mat-button routerLink="/login">Sign in</a>
              </p>
            </mat-card-content>
          </mat-card>
        } @else {
          <mat-card class="reset-card reset-success">
            <mat-card-content>
              <mat-icon class="success-icon">check_circle</mat-icon>
              <h2>Check your email</h2>
              <p>We've sent a password reset link to <strong>{{ email }}</strong>. Click the link to reset your password.</p>
              <button mat-raised-button color="primary" routerLink="/login" class="full-width-button">
                Back to Sign In
              </button>
            </mat-card-content>
          </mat-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .reset-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background-color: var(--color-bg-secondary);
      padding: var(--space-md);

      @media (min-width: 960px) {
        padding: var(--space-xl);
      }
    }

    .reset-container {
      width: 100%;
      max-width: 400px;
      text-align: center;
    }

    .logo {
      font: 500 32px/40px var(--font-family-base);
      color: var(--color-text-primary);
      letter-spacing: -0.02em;
      margin-bottom: var(--space-xl);
      display: flex;
      align-items: center;
      justify-content: center;
      gap: var(--space-sm);

      .logo-icon {
        font-size: 36px;
        width: 36px;
        height: 36px;
        color: var(--color-primary);
      }

      @media (min-width: 960px) {
        font-size: 40px;
        line-height: 48px;
        margin-bottom: var(--space-2xl);
      }
    }

    .reset-card {
      background-color: var(--color-surface-default);
      border-radius: var(--radius-xl) !important;
      box-shadow: var(--shadow-3);
      padding: var(--space-lg);
      text-align: left;

      @media (min-width: 960px) {
        padding: var(--space-xl);
      }

      .mat-mdc-card-title {
        font: var(--font-h2);
        color: var(--color-text-primary);
        margin-bottom: var(--space-lg);
        text-align: center;
      }

      .instructions {
        font: var(--font-body);
        color: var(--color-text-secondary);
        margin-bottom: var(--space-lg);
        text-align: center;
      }

      .full-width {
        width: 100%;
        margin-bottom: var(--space-md);
      }
    }

    .full-width-button {
      width: 100%;
      height: 48px;
      margin-bottom: var(--space-md);

      mat-spinner {
        display: inline-block;
        margin-right: var(--space-sm);
      }
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: var(--space-sm);
      margin-top: var(--space-sm);
      padding: var(--space-sm) var(--space-md);
      background-color: var(--color-error-bg);
      border-radius: var(--radius-md);
      font: var(--font-body-small);
      color: var(--color-error);

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
        flex-shrink: 0;
      }
    }

    .signin-prompt {
      text-align: center;
      margin-top: var(--space-lg);
      margin-bottom: 0;
      font: var(--font-body);
      color: var(--color-text-secondary);

      a {
        color: var(--color-accent);
        text-transform: none;
        padding: 0;
        min-width: auto;

        &:hover {
          text-decoration: underline;
          background: none;
        }
      }
    }

    .reset-success {
      text-align: center;

      .success-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        color: var(--color-success);
        margin-bottom: var(--space-md);
      }

      h2 {
        font: var(--font-h3);
        color: var(--color-text-primary);
        margin-bottom: var(--space-sm);
      }

      p {
        font: var(--font-body);
        color: var(--color-text-secondary);
        margin-bottom: var(--space-lg);

        strong {
          color: var(--color-text-primary);
        }
      }
    }
  `]
})
export class ResetPasswordComponent {
  email = '';
  loading = false;
  error = '';
  emailSent = false;

  constructor(private authService: AuthService) {}

  async sendResetLink() {
    if (!this.email.trim()) {
      this.error = 'Email is required';
      return;
    }

    this.error = '';
    this.loading = true;

    try {
      await this.authService.resetPassword(this.email);
      this.emailSent = true;
    } catch (err: any) {
      this.error = err.message || 'Failed to send reset email';
    } finally {
      this.loading = false;
    }
  }
}
