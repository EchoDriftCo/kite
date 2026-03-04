import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AuthService } from '../../services/auth.service';
import { SignUpDialogComponent } from './sign-up-dialog.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDialogModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error = '';
  successMessage = '';
  returnUrl = '/';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  async signIn() {
    this.error = '';
    this.successMessage = '';
    this.loading = true;

    try {
      await this.authService.signIn(this.email, this.password);
      this.router.navigateByUrl(this.returnUrl);
    } catch (err: any) {
      this.error = err.message || 'Failed to sign in';
    } finally {
      this.loading = false;
    }
  }

  async forgotPassword() {
    if (!this.email) {
      this.error = 'Enter your email address first';
      return;
    }
    this.error = '';
    this.loading = true;
    try {
      await this.authService.resetPassword(this.email);
      this.successMessage = 'Password reset email sent! Check your inbox.';
    } catch (err: any) {
      this.error = err.message || 'Failed to send reset email';
    } finally {
      this.loading = false;
    }
  }

  openSignUp() {
    const dialogRef = this.dialog.open(SignUpDialogComponent, {
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.success) {
        this.successMessage = 'Account created! Check your email to confirm your account.';
      }
    });
  }
}
