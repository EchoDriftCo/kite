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

  // New visitors explore as a guest (Supabase anonymous sign-in). Browsers that have used a
  // real account before go to login, so an expired session doesn't land them in an empty guest account.
  if (!authService.hasSignedInBefore()) {
    try {
      await authService.signInAsGuest();
      return true;
    } catch {
      // Anonymous sign-ins disabled or rate limited — fall back to login
    }
  }

  // Redirect to login with return url
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
