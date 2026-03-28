import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, User } from '../models/models';
import { environment } from '../../../environments/environment';

const TOKEN_KEY = 'token';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) {}

  /** Authenticate via POST and store the JWT token in localStorage. */
  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request).pipe(
      tap(response => localStorage.setItem(TOKEN_KEY, response.token))
    );
  }

  /** Return the current user profile from the API. */
  me(): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/me`);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return false;
      const payload = JSON.parse(atob(parts[1]));
      return typeof payload.exp === 'number' && payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getRoles(): string[] {
    const token = this.getToken();
    if (!token) return [];
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return [];
      const payload = JSON.parse(atob(parts[1]));
      const roles = payload[ROLE_CLAIM];
      if (!roles) return [];
      return Array.isArray(roles) ? roles : [roles];
    } catch {
      return [];
    }
  }

  isAdmin(): boolean {
    return this.getRoles().includes('Administrator');
  }
}

