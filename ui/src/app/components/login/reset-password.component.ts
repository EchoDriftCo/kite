import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';

// Custom validator for password match
function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('newPassword');
  const confirmPassword = control.get('confirmPassword');
  
  if (!password || !confirmPassword) {
    return null;
  }
  
  return password.value === confirmPassword.value ? null : { mismatch: true };
}

@Component({
  selector: 'app-reset-password',
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

        <mat-card class="reset-card">
          <mat-card-header>
            <mat-card-title>Set New Password</mat-card-title>
          </mat-card-header>

          <mat-card-content>
            @if (success) {
              <div class="success-container">
                <mat-icon class="success-icon">check_circle</mat-icon>
                <h2>Password Updated</h2>
                <p>Your password has been successfully updated. Redirecting...</p>
              </div>
            } @else {
              <form [formGroup]="resetForm" (ngSubmit)="onSubmit()">
                <mat-form-field appearance="outline">
                  <mat-label>New Password</mat-label>
                  <input matInput
                         [type]="hidePassword ? 'password' : 'text'"
                         formControlName="newPassword"
                         autocomplete="new-password"
                         required />
                  <button mat-icon-button 
                          matSuffix 
                          type="button"
                          (click)="hidePassword = !hidePassword"
                          [attr.aria-label]="hidePassword ? 'Show password' : 'Hide password'">
                    <mat-icon>{{ hidePassword ? 'visibility' : 'visibility_off' }}</mat-icon>
                  </button>
                  <mat-hint>Must be at least 6 characters</mat-hint>
                  @if (resetForm.get('newPassword')?.hasError('required')) {
                    <mat-error>Password is required</mat-error>
                  }
                  @if (resetForm.get('newPassword')?.hasError('minlength')) {
                    <mat-error>Password must be at least 6 characters</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Confirm Password</mat-label>
                  <input matInput
                         [type]="hideConfirmPassword ? 'password' : 'text'"
                         formControlName="confirmPassword"
                         autocomplete="new-password"
                         required />
                  <button mat-icon-button 
                          matSuffix 
                          type="button"
                          (click)="hideConfirmPassword = !hideConfirmPassword"
                          [attr.aria-label]="hideConfirmPassword ? 'Show password' : 'Hide password'">
                    <mat-icon>{{ hideConfirmPassword ? 'visibility' : 'visibility_off' }}</mat-icon>
                  </button>
                  @if (resetForm.get('confirmPassword')?.hasError('required')) {
                    <mat-error>Please confirm your password</mat-error>
                  }
                  @if (resetForm.hasError('mismatch') && resetForm.get('confirmPassword')?.touched) {
                    <mat-error>Passwords do not match</mat-error>
                  }
                </mat-form-field>

                @if (error) {
                  <div class="error-message" role="alert" aria-live="assertive">
                    <mat-icon>error</mat-icon>
                    <span>{{ error }}</span>
                  </div>
                }

                <button mat-raised-button
                        color="primary"
                        type="submit"
                        [disabled]="loading || !resetForm.valid">
                  {{ loading ? 'Updating password...' : 'Update Password' }}
                </button>
              </form>
            }
          </mat-card-content>
        </mat-card>
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

      mat-form-field {
        width: 100%;
        margin-bottom: var(--space-md);
      }

      button[type="submit"] {
        width: 100%;
        height: 48px;
        margin-top: var(--space-sm);
      }

      .error-message {
        display: flex;
        align-items: center;
        gap: var(--space-sm);
        margin-bottom: var(--space-md);
        font: var(--font-caption);
        color: var(--color-error);
        animation: fadeIn var(--transition-fast);

        mat-icon {
          font-size: 16px;
          width: 16px;
          height: 16px;
        }
      }

      .success-container {
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
        }
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
export class ResetPasswordComponent {
  resetForm: FormGroup;
  loading = false;
  error = '';
  success = false;
  hidePassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.resetForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: passwordMatchValidator });
  }

  async onSubmit() {
    if (!this.resetForm.valid) {
      return;
    }

    this.error = '';
    this.loading = true;

    try {
      const { newPassword } = this.resetForm.value;
      await this.authService.updatePassword(newPassword);
      this.success = true;
      setTimeout(() => this.router.navigate(['/recipes']), 2000);
    } catch (err: any) {
      this.error = err.message || 'Failed to update password';
    } finally {
      this.loading = false;
    }
  }
}
