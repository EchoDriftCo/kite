import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-sign-up-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <h2 mat-dialog-title>Create Account</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Email</mat-label>
        <input matInput type="email" [(ngModel)]="email" name="email">
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Password</mat-label>
        <input matInput type="password" [(ngModel)]="password" name="password">
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Confirm Password</mat-label>
        <input matInput type="password" [(ngModel)]="confirmPassword" name="confirmPassword">
      </mat-form-field>

      @if (error) {
        <div class="error-message">{{ error }}</div>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close [disabled]="loading">Cancel</button>
      <button mat-raised-button color="primary" (click)="signUp()" [disabled]="loading">
        @if (loading) {
          <mat-spinner diameter="20"></mat-spinner>
        } @else {
          Create Account
        }
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 8px;
    }
    .error-message {
      color: #f44336;
      margin-bottom: 8px;
      padding: 8px;
      background-color: rgba(244, 67, 54, 0.1);
      border-radius: 4px;
    }
    mat-dialog-content {
      min-width: 320px;
    }
  `]
})
export class SignUpDialogComponent {
  email = '';
  password = '';
  confirmPassword = '';
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private dialogRef: MatDialogRef<SignUpDialogComponent>
  ) {}

  async signUp() {
    this.error = '';

    if (!this.email.trim() || !this.password) {
      this.error = 'Email and password are required';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }

    if (this.password.length < 6) {
      this.error = 'Password must be at least 6 characters';
      return;
    }

    this.loading = true;

    try {
      await this.authService.signUp(this.email, this.password);
      this.dialogRef.close({ success: true });
    } catch (err: any) {
      this.error = err.message || 'Failed to create account';
    } finally {
      this.loading = false;
    }
  }
}
