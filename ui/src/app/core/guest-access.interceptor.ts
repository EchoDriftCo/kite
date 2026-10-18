import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { GuestPromptService } from '../services/guest-prompt.service';

/** ProblemDetails types returned by the API for guest (anonymous) users */
export const REGISTRATION_REQUIRED_TYPE = 'urn:recipevault:registration-required';
export const GUEST_QUOTA_EXCEEDED_TYPE = 'urn:recipevault:guest-quota-exceeded';

/**
 * Turns guest-only API refusals into a sign-up prompt. The error is still passed on to the caller.
 */
export const guestAccessInterceptor: HttpInterceptorFn = (req, next) => {
  const guestPrompt = inject(GuestPromptService);
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        const type = err.error?.type;
        if (err.status === 403 && type === REGISTRATION_REQUIRED_TYPE) {
          guestPrompt.promptSignUp(err.error?.detail);
        } else if (err.status === 429 && type === GUEST_QUOTA_EXCEEDED_TYPE) {
          // Open after the caller's error handler runs, so its own failure snackbar doesn't replace this one
          setTimeout(() => {
            snackBar.open(err.error?.detail ?? "You've reached today's guest limit.", 'Create account', { duration: 8000 })
              .onAction().subscribe(() => guestPrompt.promptSignUp());
          });
        }
      }
      return throwError(() => err);
    })
  );
};
