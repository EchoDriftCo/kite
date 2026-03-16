import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export interface ValidateCodeResponse {
  valid: boolean;
  message?: string;
}

export interface RedeemCodeResponse {
  success: boolean;
  message?: string;
}

@Injectable({
  providedIn: 'root'
})
export class BetaInviteCodeService {
  private baseUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private auth: AuthService
  ) {}

  validate(code: string): Observable<ValidateCodeResponse> {
    return this.http.post<ValidateCodeResponse>(
      `${this.baseUrl}/beta-invite-codes/validate`,
      { code },
      { headers: new HttpHeaders({ 'Content-Type': 'application/json' }) }
    );
  }

  redeem(code: string): Observable<RedeemCodeResponse> {
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const token = this.auth.accessToken;
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    return this.http.post<RedeemCodeResponse>(
      `${this.baseUrl}/beta-invite-codes/redeem`,
      { code },
      { headers }
    );
  }
}
