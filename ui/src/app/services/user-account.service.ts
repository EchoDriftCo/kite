import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export interface UserAccountModel {
  userAccountResourceId: string;
  subjectId: string;
  accountTier: string;
  createdDate: string;
  tierChangedDate: string | null;
  betaCodeRedeemedDate: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class UserAccountService {
  private baseUrl = environment.apiUrl;
  private accountSubject = new BehaviorSubject<UserAccountModel | null>(null);
  public account$ = this.accountSubject.asObservable();

  constructor(
    private http: HttpClient,
    private auth: AuthService
  ) {}

  /**
   * Fetch the current user's account from the backend API.
   * Caches the result in the BehaviorSubject.
   */
  getAccount(): Observable<UserAccountModel> {
    let headers = new HttpHeaders();
    const token = this.auth.accessToken;
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    return this.http.get<UserAccountModel>(
      `${this.baseUrl}/account`,
      { headers }
    ).pipe(
      tap(account => this.accountSubject.next(account))
    );
  }

  /**
   * Check if the current user has redeemed a beta code (server-side state).
   */
  get hasRedeemedBetaCode(): boolean {
    return !!this.accountSubject.value?.betaCodeRedeemedDate;
  }

  /**
   * Get the cached account tier from the backend.
   */
  get accountTier(): string {
    return this.accountSubject.value?.accountTier || 'Free';
  }

  /**
   * Clear cached account data (e.g. on sign out).
   */
  clear(): void {
    this.accountSubject.next(null);
  }
}
