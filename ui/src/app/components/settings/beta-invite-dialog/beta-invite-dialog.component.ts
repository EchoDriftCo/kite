import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BetaInviteCodeService } from '../../../services/beta-invite-code.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-beta-invite-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>vpn_key</mat-icon>
      Redeem Beta Invite Code
    </h2>
    <mat-dialog-content>
      <p class="description">Enter your beta invite code to unlock additional features.</p>

      <div class="code-input-row">
        <mat-form-field appearance="outline" class="code-segment">
          <mat-label>XXXX</mat-label>
          <input matInput
                 [(ngModel)]="segment1"
                 maxlength="4"
                 (input)="onSegmentInput(1)"
                 (paste)="onPaste($event)"
                 #seg1Input
                 [disabled]="redeeming || redeemed"
                 autocomplete="off"
                 spellcheck="false">
        </mat-form-field>
        <span class="separator">-</span>
        <mat-form-field appearance="outline" class="code-segment">
          <mat-label>XXXX</mat-label>
          <input matInput
                 [(ngModel)]="segment2"
                 maxlength="4"
                 (input)="onSegmentInput(2)"
                 (paste)="onPaste($event)"
                 #seg2Input
                 [disabled]="redeeming || redeemed"
                 autocomplete="off"
                 spellcheck="false">
        </mat-form-field>
        <span class="separator">-</span>
        <mat-form-field appearance="outline" class="code-segment">
          <mat-label>XXXX</mat-label>
          <input matInput
                 [(ngModel)]="segment3"
                 maxlength="4"
                 (input)="onSegmentInput(3)"
                 #seg3Input
                 [disabled]="redeeming || redeemed"
                 autocomplete="off"
                 spellcheck="false">
        </mat-form-field>
      </div>

      @if (validating) {
        <div class="status-row">
          <mat-spinner diameter="20"></mat-spinner>
          <span>Validating code...</span>
        </div>
      }

      @if (error) {
        <div class="status-row error">
          <mat-icon>error_outline</mat-icon>
          <span>{{ error }}</span>
        </div>
      }

      @if (validated && !error) {
        <div class="status-row success">
          <mat-icon>check_circle</mat-icon>
          <span>Code is valid! Click Redeem to activate.</span>
        </div>
      }

      @if (redeemed) {
        <div class="status-row success redeemed">
          <mat-icon>celebration</mat-icon>
          <span>Code redeemed successfully! Your account has been upgraded.</span>
        </div>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close(redeemed)">
        {{ redeemed ? 'Done' : 'Cancel' }}
      </button>
      @if (!redeemed && !validated) {
        <button mat-raised-button color="accent"
                [disabled]="!isCodeComplete || validating"
                (click)="validateCode()">
          @if (validating) {
            <mat-spinner diameter="20"></mat-spinner>
          } @else {
            <mat-icon>verified</mat-icon>
            Validate
          }
        </button>
      }
      @if (!redeemed && validated) {
        <button mat-raised-button color="primary"
                [disabled]="redeeming || !!error"
                (click)="redeem()">
          @if (redeeming) {
            <mat-spinner diameter="20"></mat-spinner>
          } @else {
            <mat-icon>redeem</mat-icon>
            Redeem Code
          }
        </button>
      }
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 8px;

      mat-icon {
        color: #f59e0b;
      }
    }

    .description {
      margin: 0 0 20px;
      color: #94a3b8;
      font-size: 14px;
    }

    .code-input-row {
      display: flex;
      align-items: center;
      gap: 4px;
      justify-content: center;
    }

    .code-segment {
      width: 90px;

      input {
        text-align: center;
        font-family: monospace;
        font-size: 18px;
        letter-spacing: 2px;
        text-transform: uppercase;
      }
    }

    .separator {
      font-size: 24px;
      color: #64748b;
      margin-bottom: 22px;
    }

    .status-row {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-top: 16px;
      font-size: 14px;

      &.error {
        color: #ef4444;
      }

      &.success {
        color: #22c55e;
      }

      &.redeemed {
        font-weight: 500;
        font-size: 15px;
      }
    }

    mat-dialog-actions button mat-spinner {
      display: inline-block;
    }

    @media (max-width: 480px) {
      .code-segment {
        width: 70px;

        input {
          font-size: 15px;
        }
      }
    }
  `]
})
export class BetaInviteDialogComponent {
  segment1 = '';
  segment2 = '';
  segment3 = '';

  validating = false;
  validated = false;
  redeeming = false;
  redeemed = false;
  error = '';

  constructor(
    public dialogRef: MatDialogRef<BetaInviteDialogComponent>,
    private betaInviteCodeService: BetaInviteCodeService,
    private authService: AuthService
  ) {}

  get fullCode(): string {
    return `${this.segment1}-${this.segment2}-${this.segment3}`.toUpperCase();
  }

  get isCodeComplete(): boolean {
    return this.segment1.length === 4 && this.segment2.length === 4 && this.segment3.length === 4;
  }

  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pasted = (event.clipboardData?.getData('text') || '').trim().toUpperCase();
    // Handle full code paste: XXXX-XXXX-XXXX or XXXXXXXXXXXX
    const cleaned = pasted.replace(/[^A-Z0-9]/g, '');
    if (cleaned.length >= 12) {
      this.segment1 = cleaned.substring(0, 4);
      this.segment2 = cleaned.substring(4, 8);
      this.segment3 = cleaned.substring(8, 12);
    }
  }

  onSegmentInput(segment: number): void {
    this.error = '';
    this.validated = false;

    // Auto-advance to next segment
    if (segment === 1 && this.segment1.length === 4) {
      this.focusSegment(2);
    } else if (segment === 2 && this.segment2.length === 4) {
      this.focusSegment(3);
    }
  }

  private focusSegment(segment: number): void {
    setTimeout(() => {
      const inputs = document.querySelectorAll<HTMLInputElement>('app-beta-invite-dialog input');
      if (inputs[segment - 1]) {
        inputs[segment - 1].focus();
      }
    });
  }

  validateCode(): void {
    if (!this.isCodeComplete) return;

    this.validating = true;
    this.error = '';
    this.validated = false;

    this.betaInviteCodeService.validate(this.fullCode).subscribe({
      next: (response) => {
        this.validating = false;
        if (response.isValid) {
          this.validated = true;
        } else {
          this.error = response.message || 'Invalid invite code';
        }
      },
      error: (err) => {
        this.validating = false;
        this.error = err.error?.message || err.error?.detail || 'Failed to validate code';
      }
    });
  }

  redeem(): void {
    if (!this.validated || this.redeeming) return;

    this.redeeming = true;
    this.error = '';

    this.betaInviteCodeService.redeem(this.fullCode).subscribe({
      next: () => {
        this.redeeming = false;
        this.redeemed = true;
        // Refresh session to pick up updated user tier
        this.authService.refreshSession().catch(() => {});
      },
      error: (err) => {
        this.redeeming = false;
        this.error = err.error?.message || err.error?.detail || 'Failed to redeem code';
      }
    });
  }
}
