import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../services/auth.service';

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
    MatProgressSpinnerModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error = '';
  returnUrl = '/';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  async signIn() {
    this.error = '';
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

  async signUp() {
    this.error = '';
    this.loading = true;

    try {
      await this.authService.signUp(this.email, this.password);
      this.error = 'Check your email to confirm your account!';
    } catch (err: any) {
      this.error = err.message || 'Failed to sign up';
    } finally {
      this.loading = false;
    }
  }
}
