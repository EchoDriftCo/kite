import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { SupabaseService } from './supabase.service';

describe('AuthService guest sign-in', () => {
  let auth: jasmine.SpyObj<any>;

  function setup(existingSession: any) {
    auth = jasmine.createSpyObj('auth', ['getSession', 'onAuthStateChange', 'signInAnonymously']);
    auth.getSession.and.resolveTo({ data: { session: existingSession } });
    auth.signInAnonymously.and.resolveTo({
      data: { session: { user: { id: 'new-guest', is_anonymous: true } } },
      error: null
    });

    TestBed.configureTestingModule({
      providers: [
        { provide: SupabaseService, useValue: { client: { auth } } },
        { provide: Router, useValue: jasmine.createSpyObj('Router', ['navigate']) }
      ]
    });
    return TestBed.inject(AuthService);
  }

  it('reuses a restored guest session instead of creating a new guest', async () => {
    // Called before the saved session has been restored, e.g. from the login page
    const service = setup({ user: { id: 'existing-guest', is_anonymous: true } });

    await service.signInAsGuest();

    expect(auth.signInAnonymously).not.toHaveBeenCalled();
    expect(service.currentUser?.id).toBe('existing-guest');
  });

  it('creates one guest session when there is none, even with concurrent callers', async () => {
    const service = setup(null);

    await Promise.all([service.signInAsGuest(), service.signInAsGuest()]);

    expect(auth.signInAnonymously).toHaveBeenCalledTimes(1);
    expect(service.isGuest()).toBeTrue();
  });
});
