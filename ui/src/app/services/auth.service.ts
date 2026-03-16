import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { SupabaseService } from './supabase.service';
import { User, Session } from '@supabase/supabase-js';

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

  constructor(
    private supabase: SupabaseService,
    private router: Router
  ) {
    // Initialize auth state
    this.ready = this.supabase.client.auth.getSession().then(({ data: { session } }) => {
      this.sessionSubject.next(session);
      this.currentUserSubject.next(session?.user ?? null);
    });

    // Listen for auth changes
    this.supabase.client.auth.onAuthStateChange((event, session) => {
      this.sessionSubject.next(session);
      this.currentUserSubject.next(session?.user ?? null);
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
    this.sessionSubject.next(session);
    this.currentUserSubject.next(session?.user ?? null);
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!this.currentUser;
  }
}
