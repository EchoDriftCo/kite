import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ConfirmService } from './confirm.service';
import { ConfirmDialogComponent } from '../shared/components/confirm-dialog/confirm-dialog.component';

describe('ConfirmService', () => {
  let service: ConfirmService;
  let dialog: MatDialog;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideNoopAnimations()] });
    service = TestBed.inject(ConfirmService);
    dialog = TestBed.inject(MatDialog);
  });

  afterEach(() => dialog.closeAll());

  function openDialog(data: Parameters<ConfirmService['ask']>[0]) {
    const result = service.ask(data);
    const ref = dialog.openDialogs[0];
    ref.componentRef!.changeDetectorRef.detectChanges();
    return { result, ref, component: ref.componentInstance as ConfirmDialogComponent };
  }

  it('resolves true when confirmed', async () => {
    const { result, component } = openDialog({ title: 'Delete recipe?', message: 'Gone.', confirmText: 'Delete', destructive: true });
    component.close(true);
    expect(await result).toBeTrue();
  });

  it('resolves false when dismissed without a result (Esc or backdrop)', async () => {
    const { result, ref } = openDialog({ title: 'Delete recipe?', message: 'Gone.', confirmText: 'Delete' });
    ref.close();
    expect(await result).toBeFalse();
  });

  it('keeps confirm disabled until the required text is typed', () => {
    const { component } = openDialog({
      title: 'Delete your account?', message: 'Gone.', confirmText: 'Delete account', destructive: true, requireText: 'DELETE',
    });
    expect(component.canConfirm).toBeFalse();
    component.typed = 'delete';
    expect(component.canConfirm).toBeFalse();
    component.typed = 'DELETE';
    expect(component.canConfirm).toBeTrue();
  });

  it('labels the confirm button with the action verb', () => {
    const { ref } = openDialog({ title: 'Leave circle?', message: 'Bye.', confirmText: 'Leave', destructive: true });
    const buttons = Array.from(ref.componentRef!.location.nativeElement.querySelectorAll('button')) as HTMLElement[];
    expect(buttons.map(b => b.textContent!.trim())).toEqual(['Cancel', 'Leave']);
  });
});
