import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { SupabaseService } from './supabase.service';
import { User, Session } from '@supabase/supabase-js';

/** Set once this browser has signed in with a real account, so an expired session goes to login instead of a new guest session. */
const HAS_ACCOUNT_KEY = 'rv-has-account';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private sessionSubject = new BehaviorSubject<Session | null>(null);
  public session$ = this.sessionSubject.asObservable();

  /** Resolves once the initial session check has completed. */
  public readonly ready: Promise<void>;

  private guestSignIn?: Promise<void>;

  constructor(
    private supabase: SupabaseService,
    private router: Router
  ) {
    // Initialize auth state
    this.ready = this.supabase.client.auth.getSession().then(({ data: { session } }) => {
      this.setSession(session);
    });

    // Listen for auth changes
    this.supabase.client.auth.onAuthStateChange((event, session) => {
      this.setSession(session);

      if (event === 'PASSWORD_RECOVERY') {
        this.router.navigate(['/update-password']);
      }
    });
  }

  /**
   * Get the current session
   */
  get session(): Session | null {
    return this.sessionSubject.value;
  }

  /**
   * Get the current user
   */
  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Get the current JWT access token
   */
  get accessToken(): string | null {
    return this.session?.access_token ?? null;
  }

  /**
   * Sign up with email and password
   */
  async signUp(email: string, password: string) {
    const { data, error } = await this.supabase.client.auth.signUp({
      email,
      password
    });

    if (error) throw error;
    return data;
  }

  /**
   * Sign in with email and password
   */
  async signIn(email: string, password: string) {
    const { data, error } = await this.supabase.client.auth.signInWithPassword({
      email,
      password
    });

    if (error) throw error;
    return data;
  }

  /**
   * Start a guest session (Supabase anonymous sign-in) so a visitor can use the app without signing up.
   * Concurrent callers share one in-flight request.
   */
  signInAsGuest(): Promise<void> {
    if (!this.guestSignIn) {
      this.guestSignIn = this.supabase.client.auth.signInAnonymously()
        .then(({ data, error }) => {
          if (error) throw error;
          this.setSession(data.session);
        })
        .finally(() => this.guestSignIn = undefined);
    }
    return this.guestSignIn;
  }

  /**
   * Turn the current guest into a full account. The user id stays the same, so everything they made is kept.
   * When the project requires email confirmation the email change is pending until confirmed, and the
   * password can't be set yet (they can sign in with a magic link or reset their password afterwards).
   */
  async convertGuest(email: string, password: string): Promise<{ needsEmailConfirmation: boolean }> {
    const { data, error } = await this.supabase.client.auth.updateUser({ email });
    if (error) throw error;

    if (data.user?.email !== email) {
      return { needsEmailConfirmation: true };
    }

    const { error: passwordError } = await this.supabase.client.auth.updateUser({ password });
    if (passwordError) throw passwordError;

    // Pick up a fresh token without the is_anonymous claim
    await this.refreshSession();
    return { needsEmailConfirmation: false };
  }

  /**
   * True when this browser has signed in with a real account before
   */
  hasSignedInBefore(): boolean {
    try {
      return localStorage.getItem(HAS_ACCOUNT_KEY) === 'true';
    } catch {
      return false;
    }
  }

  private setSession(session: Session | null): void {
    this.sessionSubject.next(session);
    this.currentUserSubject.next(session?.user ?? null);
    if (session?.user && !session.user.is_anonymous) {
      this.rememberAccount();
    }
  }

  private rememberAccount(): void {
    try {
      localStorage.setItem(HAS_ACCOUNT_KEY, 'true');
    } catch {
      // Storage unavailable (private mode) — guests will just get a new guest session
    }
  }

  /**
   * Sign out
   */
  async signOut() {
    const { error } = await this.supabase.client.auth.signOut();
    if (error) throw error;
    this.router.navigate(['/login']);
  }

  /**
   * Send password reset email
   */
  async resetPassword(email: string) {
    const { error } = await this.supabase.client.auth.resetPasswordForEmail(email);
    if (error) throw error;
  }

  /**
   * Sign in with magic link (OTP via email)
   */
  async signInWithOtp(email: string) {
    const { error } = await this.supabase.client.auth.signInWithOtp({ email });
    if (error) throw error;
  }

  /**
   * Update user password
   */
  async updatePassword(newPassword: string) {
    const { error } = await this.supabase.client.auth.updateUser({
      password: newPassword
    });
    if (error) throw error;
  }

  /**
   * Refresh the current session to pick up updated user metadata (e.g. tier changes).
   */
  async refreshSession(): Promise<void> {
    const { data: { session }, error } = await this.supabase.client.auth.refreshSession();
    if (error) throw error;
    this.setSession(session);
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!this.currentUser;
  }

  /**
   * Check if the current user is a guest (anonymous sign-in, no account yet)
   */
  isGuest(): boolean {
    return !!this.currentUser?.is_anonymous;
  }

  /**
   * Get the user's subscription tier from JWT app_metadata.
   * Returns 'Free' when no tier is set.
   */
  getUserTier(): string {
    return (this.currentUser as any)?.app_metadata?.tier || 'Free';
  }
}
