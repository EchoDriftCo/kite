import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CircleService } from '../../../services/circle.service';
import { Circle } from '../../../models/circle.model';
import { Clipboard } from '@angular/cdk/clipboard';

export interface InviteDialogData {
  circle: Circle;
}

@Component({
  selector: 'app-invite-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './invite-dialog.component.html',
  styleUrl: './invite-dialog.component.scss'
})
export class InviteDialogComponent {
  emailForm: FormGroup;
  inviteLink = '';
  sendingEmail = false;
  generatingLink = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private circleService: CircleService,
    private clipboard: Clipboard,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<InviteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: InviteDialogData
  ) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  sendEmailInvite() {
    if (this.emailForm.invalid) {
      return;
    }

    this.sendingEmail = true;
    this.error = '';

    const inviteeEmail = this.emailForm.value.email;

    this.circleService.inviteMember(this.data.circle.circleResourceId, { inviteeEmail }).subscribe({
      next: () => {
        this.sendingEmail = false;
        this.snackBar.open('Invitation sent!', 'Close', { duration: 3000 });
        this.emailForm.reset();
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.error = err.message || 'Failed to send invitation';
        this.sendingEmail = false;
        console.error('Error sending invitation:', err);
      }
    });
  }

  generateInviteLink() {
    this.generatingLink = true;
    this.error = '';

    this.circleService.inviteMember(this.data.circle.circleResourceId, {}).subscribe({
      next: (invite) => {
        this.inviteLink = invite.inviteToken ? `${window.location.origin}/join/${invite.inviteToken}` : '';
        this.generatingLink = false;
        this.snackBar.open('Invite link generated!', 'Close', { duration: 2000 });
      },
      error: (err) => {
        this.error = err.message || 'Failed to generate invite link';
        this.generatingLink = false;
        console.error('Error generating invite link:', err);
      }
    });
  }

  copyInviteLink() {
    if (this.inviteLink) {
      this.clipboard.copy(this.inviteLink);
      this.snackBar.open('Link copied to clipboard!', 'Close', { duration: 2000 });
    }
  }

  onClose() {
    this.dialogRef.close();
  }
}
