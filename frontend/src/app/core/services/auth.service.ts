import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_CONFIG } from '../config/api.config';
import { LoginRequest } from '../models/auth/login-request.model';
import { RegisterRequest } from '../models/auth/register-request.model';
import { AuthResponse } from '../models/auth/auth-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly authUrl = `${API_CONFIG.baseUrl}/auth`;
  private readonly accessTokenKey = 'accessToken';
  private readonly roleKey = 'role';

  private readonly _isAuthenticated = signal<boolean>(this.hasToken());
  readonly isAuthenticated = computed(() => this._isAuthenticated());

  private readonly _role = signal<string>(this.getStoredRole());
  readonly role = computed(() => this._role());

  // true only when the logged-in user is an Admin
  readonly isAdmin = computed(() => this._role() === 'Admin');

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.authUrl}/register`, request, { withCredentials: true })
      .pipe(tap(res => this.storeAuth(res)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.authUrl}/login`, request, { withCredentials: true })
      .pipe(tap(res => this.storeAuth(res)));
  }

  refresh(): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.authUrl}/refresh`, {}, { withCredentials: true })
      .pipe(tap(res => this.storeAuth(res)));
  }

  logout(): Observable<unknown> {
    return this.http
      .post(`${this.authUrl}/logout`, {}, { withCredentials: true })
      .pipe(tap(() => this.clearLocalAuth()));
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  clearLocalAuth(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.roleKey);
    this._isAuthenticated.set(false);
    this._role.set('');
  }

  private storeAuth(res: AuthResponse): void {
    localStorage.setItem(this.accessTokenKey, res.accessToken);
    localStorage.setItem(this.roleKey, res.role);
    this._isAuthenticated.set(true);
    this._role.set(res.role);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.accessTokenKey);
  }

  private getStoredRole(): string {
    return localStorage.getItem(this.roleKey) ?? '';
  }
}