import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../services/auth.service';

// Custom validator for password match
function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  
  if (!password || !confirmPassword) {
    return null;
  }
  
  return password.value === confirmPassword.value ? null : { mismatch: true };
}

@Component({
  selector: 'app-sign-up-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>Create Account</h2>
      <button mat-icon-button 
              [mat-dialog-close]="false"
              aria-label="Close dialog">
        <mat-icon>close</mat-icon>
      </button>
    </div>

    <mat-dialog-content>
      <form [formGroup]="signUpForm">
        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput 
                 type="email" 
                 formControlName="email" 
                 autocomplete="email" 
                 required />
          @if (signUpForm.get('email')?.hasError('required')) {
            <mat-error>Email is required</mat-error>
          }
          @if (signUpForm.get('email')?.hasError('email')) {
            <mat-error>Invalid email format</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Password</mat-label>
          <input matInput 
                 [type]="hidePassword ? 'password' : 'text'"
                 formControlName="password"
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
          @if (signUpForm.get('password')?.hasError('required')) {
            <mat-error>Password is required</mat-error>
          }
          @if (signUpForm.get('password')?.hasError('minlength')) {
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
          @if (signUpForm.get('confirmPassword')?.hasError('required')) {
            <mat-error>Please confirm your password</mat-error>
          }
          @if (signUpForm.hasError('mismatch') && signUpForm.get('confirmPassword')?.touched) {
            <mat-error>Passwords do not match</mat-error>
          }
        </mat-form-field>

        <mat-checkbox formControlName="agreeToTerms" required class="terms-checkbox">
          I agree to the 
          <a href="/terms" target="_blank">Terms of Service</a> and 
          <a href="/privacy" target="_blank">Privacy Policy</a>
        </mat-checkbox>

        @if (error) {
          <div class="error-message" role="alert" aria-live="assertive">
            <mat-icon>error</mat-icon>
            <span>{{ error }}</span>
          </div>
        }
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="false">Cancel</button>
      <button mat-raised-button 
              color="primary"
              (click)="onCreateAccount()"
              [disabled]="loading || !signUpForm.valid">
        {{ loading ? 'Creating account...' : 'Create Account' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding-right: 8px;

      h2 {
        margin: 0;
        flex: 1;
      }
    }

    mat-dialog-content {
      min-width: 320px;
      padding: 0 24px !important;

      mat-form-field {
        width: 100%;
        margin-bottom: 16px;
      }
    }

    .terms-checkbox {
      margin: 16px 0;
      display: block;

      ::ng-deep .mdc-label {
        font: var(--font-body);
        color: var(--color-text-primary);
      }

      a {
        color: var(--color-accent);
        text-decoration: none;

        &:hover {
          text-decoration: underline;
        }
      }
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-top: 8px;
      font: var(--font-caption);
      color: var(--color-error);
      animation: fadeIn 150ms ease-in-out;

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
      }
    }

    mat-dialog-actions {
      gap: 8px;
      padding: 16px 24px !important;
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
export class SignUpDialogComponent {
  signUpForm: FormGroup;
  loading = false;
  error = '';
  hidePassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private dialogRef: MatDialogRef<SignUpDialogComponent>
  ) {
    this.signUpForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      agreeToTerms: [false, Validators.requiredTrue]
    }, { validators: passwordMatchValidator });
  }

  async onCreateAccount() {
    if (!this.signUpForm.valid) {
      return;
    }

    this.error = '';
    this.loading = true;

    try {
      const { email, password } = this.signUpForm.value;
      await this.authService.signUp(email, password);
      this.dialogRef.close({ success: true });
    } catch (err: any) {
      this.error = err.message || 'Failed to create account';
    } finally {
      this.loading = false;
    }
  }
}
