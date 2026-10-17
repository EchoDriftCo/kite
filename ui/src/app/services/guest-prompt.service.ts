import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SignUpDialogComponent, SignUpDialogData, SignUpDialogResult } from '../components/login/sign-up-dialog.component';

/**
 * Asks a guest (anonymous sign-in) to create an account, keeping everything they've made.
 */
@Injectable({
  providedIn: 'root'
})
export class GuestPromptService {
  private isOpen = false;

  constructor(
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  promptSignUp(reason?: string): void {
    if (this.isOpen) {
      return;
    }
    this.isOpen = true;

    this.dialog.open<SignUpDialogComponent, SignUpDialogData, SignUpDialogResult>(SignUpDialogComponent, {
      width: '480px',
      maxWidth: '95vw',
      data: { reason }
    }).afterClosed().subscribe(result => {
      this.isOpen = false;
      if (result?.success) {
        const message = result.needsEmailConfirmation
          ? 'Check your email to confirm your account. Your recipes are saved.'
          : 'Account created. Your recipes are saved.';
        this.snackBar.open(message, 'OK', { duration: 6000 });
      }
    });
  }
}
