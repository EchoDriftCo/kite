import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WaitlistResponse {
  message: string;
  resourceId: string;
}

@Injectable({
  providedIn: 'root'
})
export class WaitlistService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  joinWaitlist(email: string, source: string = 'features-page'): Observable<WaitlistResponse> {
    return this.http.post<WaitlistResponse>(
      `${this.baseUrl}/waitlist`,
      { email, source },
      { headers: { 'Content-Type': 'application/json' } }
    );
  }
}
