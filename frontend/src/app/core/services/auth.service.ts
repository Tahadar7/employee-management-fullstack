import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { LoginRequest } from '../models/auth/login-request.model';
import { RegisterRequest } from '../models/auth/register-request.model';
import { AuthResponse } from '../models/auth/auth-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  private readonly authUrl = `${API_CONFIG.baseUrl}/auth`;
  private readonly accessTokenKey = 'accessToken';

  private readonly _isAuthenticated = signal<boolean>(this.hasToken());
  readonly isAuthenticated = computed(() => this._isAuthenticated());

  register(request: RegisterRequest): Observable<AuthResponse> {
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
    return this.http
      .post<AuthResponse>(`${this.authUrl}/refresh`, {}, { withCredentials: true })
      .pipe(tap(res => this.storeAccessToken(res)));
  }

  logout(): Observable<unknown> {
    return this.http
      .post(`${this.authUrl}/logout`, {}, { withCredentials: true })
      .pipe(tap(() => this.clearLocalAuth()));
  }

  getAccessToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(this.accessTokenKey);
  }

  clearLocalAuth(): void {
    if (this.isBrowser) {
      localStorage.removeItem(this.accessTokenKey);
    }
    this._isAuthenticated.set(false);
  }

  private storeAccessToken(res: AuthResponse): void {
    if (this.isBrowser) {
      localStorage.setItem(this.accessTokenKey, res.accessToken);
    }
    this._isAuthenticated.set(true);
  }

  private hasToken(): boolean {
    if (!this.isBrowser) return false;
    return !!localStorage.getItem(this.accessTokenKey);
  }
}