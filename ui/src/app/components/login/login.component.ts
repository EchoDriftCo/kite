import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AuthService } from '../../services/auth.service';
import { SignUpDialogComponent } from './sign-up-dialog.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  error = '';
  successMessage = '';
  returnUrl = '/';
  hidePassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  async onSignIn() {
    if (!this.loginForm.valid) {
      return;
    }

    this.error = '';
    this.successMessage = '';
    this.loading = true;

    try {
      const { email, password } = this.loginForm.value;
      await this.authService.signIn(email, password);
      this.router.navigateByUrl(this.returnUrl);
    } catch (err: any) {
      this.error = err.message || 'Invalid email or password';
    } finally {
      this.loading = false;
    }
  }

  async onMagicLink() {
    const email = this.loginForm.get('email')?.value;
    
    if (!email) {
      this.error = 'Enter your email address first';
      return;
    }
    
    this.error = '';
    this.loading = true;
    
    try {
      await this.authService.signInWithOtp(email);
      this.successMessage = 'Magic link sent! Check your email to sign in.';
    } catch (err: any) {
      this.error = err.message || 'Failed to send magic link';
    } finally {
      this.loading = false;
    }
  }

  onSignUp() {
    const dialogRef = this.dialog.open(SignUpDialogComponent, {
      width: '480px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.success) {
        this.successMessage = 'Account created! Check your email to confirm your account.';
      }
    });
  }
}
