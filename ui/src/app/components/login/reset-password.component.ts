import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="reset-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Set New Password</mat-card-title>
        </mat-card-header>

        <mat-card-content>
          @if (success) {
            <div class="success-message">
              Password updated successfully! Redirecting...
            </div>
          } @else {
            <form (ngSubmit)="submit()">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>New Password</mat-label>
                <input matInput type="password" [(ngModel)]="newPassword" name="newPassword" required>
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Confirm Password</mat-label>
                <input matInput type="password" [(ngModel)]="confirmPassword" name="confirmPassword" required>
              </mat-form-field>

              @if (error) {
                <div class="error-message">{{ error }}</div>
              }

              <button mat-raised-button color="primary" type="submit" class="full-width submit-button" [disabled]="loading">
                @if (loading) {
                  <mat-spinner diameter="20"></mat-spinner>
                } @else {
                  Update Password
                }
              </button>
            </form>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .reset-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 20px;
    }
    mat-card {
      max-width: 400px;
      width: 100%;
    }
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    .submit-button {
      margin-top: 8px;
      height: 48px;
      font-size: 16px;
    }
    .error-message {
      color: #f44336;
      margin-bottom: 16px;
      padding: 8px;
      background-color: rgba(244, 67, 54, 0.1);
      border-radius: 4px;
    }
    .success-message {
      color: #4caf50;
      margin-bottom: 16px;
      padding: 8px;
      background-color: rgba(76, 175, 80, 0.1);
      border-radius: 4px;
    }
  `]
})
export class ResetPasswordComponent {
  newPassword = '';
  confirmPassword = '';
  loading = false;
  error = '';
  success = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async submit() {
    this.error = '';

    if (!this.newPassword) {
      this.error = 'Password is required';
      return;
    }

    if (this.newPassword.length < 6) {
      this.error = 'Password must be at least 6 characters';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }

    this.loading = true;

    try {
      await this.authService.updatePassword(this.newPassword);
      this.success = true;
      setTimeout(() => this.router.navigate(['/']), 2000);
    } catch (err: any) {
      this.error = err.message || 'Failed to update password';
    } finally {
      this.loading = false;
    }
  }
}
