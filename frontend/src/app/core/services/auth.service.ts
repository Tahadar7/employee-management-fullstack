import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { LoginRequest } from '../models/auth/login-request.model';
import { RegisterRequest } from '../models/auth/register-request.model';
import { AuthResponse } from '../models/auth/auth-response.model';

@Injectable({ 
  providedIn: 'root' 
})

export class AuthService {
  private readonly authUrl = `${API_CONFIG.baseUrl}/auth`;
  private readonly accessTokenKey = 'accessToken';

  private readonly _isAuthenticated = signal<boolean>(this.hasToken());
  readonly isAuthenticated = computed(() => this._isAuthenticated());

  constructor(private http: HttpClient) {}

  register(request: RegisterRequest): Observable<AuthResponse> {
    // withCredentials so the browser accepts the refresh cookie
    return this.http
      .post<AuthResponse>(`${this.authUrl}/register`, request, { withCredentials: true })
      .pipe(tap(res => this.storeAccessToken(res)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.authUrl}/login`, request, { withCredentials: true })
      .pipe(tap(res => this.storeAccessToken(res)));
  }

  refresh(): Observable<AuthResponse> {
    // no body — refresh token is sent automatically via the HttpOnly cookie
    return this.http
      .post<AuthResponse>(`${this.authUrl}/refresh`, {}, { withCredentials: true })
      .pipe(tap(res => this.storeAccessToken(res)));
  }

  logout(): Observable<unknown> {
    // tell the server to clear the cookie, then clear local state
    return this.http
      .post(`${this.authUrl}/logout`, {}, { withCredentials: true })
      .pipe(tap(() => this.clearLocalAuth()));
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  clearLocalAuth(): void {
    localStorage.removeItem(this.accessTokenKey);
    this._isAuthenticated.set(false);
  }

  private storeAccessToken(res: AuthResponse): void {
    localStorage.setItem(this.accessTokenKey, res.accessToken);
    this._isAuthenticated.set(true);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.accessTokenKey);
  }
}