import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = async (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for the initial session check to complete before deciding
  await authService.ready;

  if (authService.isAuthenticated()) {
    return true;
  }

  // Redirect to login with return url
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
