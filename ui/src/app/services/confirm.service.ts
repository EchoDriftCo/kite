import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import { ConfirmDialogComponent, ConfirmDialogData } from '../shared/components/confirm-dialog/confirm-dialog.component';

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  constructor(private dialog: MatDialog) {}

  /** Resolves true only when the person confirms; Cancel, Esc and backdrop clicks resolve false. */
  async ask(data: ConfirmDialogData): Promise<boolean> {
    const ref = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
      data,
      width: '400px',
      maxWidth: 'calc(100vw - 32px)',
      role: data.destructive ? 'alertdialog' : 'dialog',
    });
    return (await firstValueFrom(ref.afterClosed())) === true;
  }
}
