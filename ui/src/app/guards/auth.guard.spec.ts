import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  const runGuard = () => TestBed.runInInjectionContext(() =>
    authGuard({} as ActivatedRouteSnapshot, { url: '/recipes' } as RouterStateSnapshot)
  );

  beforeEach(() => {
    authService = jasmine.createSpyObj<AuthService>('AuthService', ['isAuthenticated', 'hasSignedInBefore', 'signInAsGuest'], {
      ready: Promise.resolve()
    });
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    });
  });

  it('allows signed-in users', async () => {
    authService.isAuthenticated.and.returnValue(true);

    expect(await runGuard()).toBeTrue();
    expect(authService.signInAsGuest).not.toHaveBeenCalled();
  });

  it('starts a guest session for new visitors', async () => {
    authService.isAuthenticated.and.returnValue(false);
    authService.hasSignedInBefore.and.returnValue(false);
    authService.signInAsGuest.and.resolveTo();

    expect(await runGuard()).toBeTrue();
    expect(authService.signInAsGuest).toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('sends returning account holders to login instead of a guest session', async () => {
    authService.isAuthenticated.and.returnValue(false);
    authService.hasSignedInBefore.and.returnValue(true);

    expect(await runGuard()).toBeFalse();
    expect(authService.signInAsGuest).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/recipes' } });
  });

  it('falls back to login when guest sign-in fails', async () => {
    authService.isAuthenticated.and.returnValue(false);
    authService.hasSignedInBefore.and.returnValue(false);
    authService.signInAsGuest.and.rejectWith(new Error('Anonymous sign-ins are disabled'));

    expect(await runGuard()).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/recipes' } });
  });
});
