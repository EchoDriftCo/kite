import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-request-reset',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="reset-password-page">
      <div class="reset-container">
        <h1 class="logo">RecipeVault</h1>

        @if (!successState) {
          <mat-card class="reset-card">
            <mat-card-header>
              <mat-card-title>Reset Password</mat-card-title>
            </mat-card-header>

            <mat-card-content>
              <p class="instructions">
                Enter your email address and we'll send you a link to reset your password.
              </p>

              <form [formGroup]="resetForm" (ngSubmit)="onSendResetLink()">
                <mat-form-field appearance="outline">
                  <mat-label>Email</mat-label>
                  <input matInput
                         type="email"
                         formControlName="email"
                         autocomplete="email"
                         placeholder="you@example.com"
                         required />
                  @if (resetForm.get('email')?.hasError('required')) {
                    <mat-error>Email is required</mat-error>
                  }
                  @if (resetForm.get('email')?.hasError('email')) {
                    <mat-error>Invalid email format</mat-error>
                  }
                </mat-form-field>

                <button mat-raised-button
                        color="primary"
                        type="submit"
                        [disabled]="loading || !resetForm.valid">
                  {{ loading ? 'Sending...' : 'SEND RESET LINK' }}
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
              <p>
                We've sent a password reset link to <strong>{{ email }}</strong>.
                Click the link to reset your password.
              </p>
              <button mat-raised-button color="primary" routerLink="/login">
                Back to Sign In
              </button>
            </mat-card-content>
          </mat-card>
        }
      </div>
    </div>
  `,
  styles: [`
    .reset-password-page {
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

      ::ng-deep .mat-mdc-card-header {
        display: block;
        padding: 0;
      }

      ::ng-deep .mat-mdc-card-title {
        font: var(--font-h2);
        color: var(--color-text-primary);
        margin-bottom: var(--space-lg);
        text-align: center;
      }

      ::ng-deep .mat-mdc-card-content {
        padding: 0;
      }

      .instructions {
        font: var(--font-body);
        color: var(--color-text-secondary);
        margin-bottom: var(--space-lg);
        text-align: center;
      }

      mat-form-field {
        width: 100%;
        margin-bottom: var(--space-md);
      }

      button[type="submit"] {
        width: 100%;
        height: 48px;
        margin-bottom: var(--space-md);
      }

      .error-message {
        display: flex;
        align-items: center;
        gap: var(--space-sm);
        margin-top: var(--space-sm);
        font: var(--font-caption);
        color: var(--color-error);
        animation: fadeIn var(--transition-fast);

        mat-icon {
          font-size: 16px;
          width: 16px;
          height: 16px;
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
    }

    .reset-success {
      text-align: center;

      .success-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        color: var(--color-success);
        margin: 0 auto var(--space-md);
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

      button {
        width: 100%;
        height: 48px;
      }
    }

    @keyframes fadeIn {
      from {
        opacity: 0;
        transform: translateY(-4px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
  `]
})
export class RequestResetComponent {
  resetForm: FormGroup;
  loading = false;
  error = '';
  successState = false;
  email = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.resetForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  async onSendResetLink() {
    if (!this.resetForm.valid) {
      return;
    }

    this.error = '';
    this.loading = true;

    try {
      this.email = this.resetForm.value.email;
      await this.authService.resetPassword(this.email);
      this.successState = true;
    } catch (err: any) {
      this.error = err.message || 'No account found with this email address.';
    } finally {
      this.loading = false;
    }
  }
}
