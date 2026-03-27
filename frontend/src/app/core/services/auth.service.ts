import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequest, LoginResponse, User } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) {}

  /** Authenticate and store the JWT token. */
  login(request: LoginRequest): Observable<LoginResponse> {
    // TODO: implement – store token in localStorage after response
    throw new Error('Not implemented');
  }

  /** Return the current user profile from the API. */
  me(): Observable<User> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  logout(): void {
    // TODO: implement – clear localStorage token
  }

  isAuthenticated(): boolean {
    // TODO: implement – check token expiry
    return false;
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getRoles(): string[] {
    // TODO: decode roles from JWT
    return [];
  }

  isAdmin(): boolean {
    return this.getRoles().includes('Administrator');
  }
}

