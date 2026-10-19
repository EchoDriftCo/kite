import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface ConfirmDialogData {
  title: string;
  message: string;
  /** Label for the confirm button; repeat the verb ("Delete"), never "OK". */
  confirmText: string;
  cancelText?: string;
  /** Red confirm button, and focus starts on Cancel. */
  destructive?: boolean;
  /** When set, the confirm button stays disabled until this exact text is typed. */
  requireText?: string;
}

/** Replaces window.confirm()/prompt(). Open it through ConfirmService. See docs/designs/tasks/5.0. */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <p class="message">{{ data.message }}</p>
      @if (data.requireText) {
        <mat-form-field appearance="outline" class="require-field">
          <mat-label>Type {{ data.requireText }} to confirm</mat-label>
          <input matInput [(ngModel)]="typed" autocomplete="off" (keyup.enter)="canConfirm && close(true)">
        </mat-form-field>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close(false)" [attr.cdkFocusInitial]="data.destructive && !data.requireText ? '' : null">
        {{ data.cancelText || 'Cancel' }}
      </button>
      <button mat-flat-button
              [color]="data.destructive ? 'warn' : 'primary'"
              [disabled]="!canConfirm"
              (click)="close(true)"
              [attr.cdkFocusInitial]="data.destructive || data.requireText ? null : ''">
        {{ data.confirmText }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .message {
      margin: 0;
      font: var(--font-body-large);
      color: var(--color-text-secondary);
    }

    .require-field {
      width: 100%;
      margin-top: var(--space-md);
    }
  `]
})
export class ConfirmDialogComponent {
  typed = '';

  constructor(
    private dialogRef: MatDialogRef<ConfirmDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}

  get canConfirm(): boolean {
    return !this.data.requireText || this.typed.trim() === this.data.requireText;
  }

  close(result: boolean): void {
    this.dialogRef.close(result);
  }
}
