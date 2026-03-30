import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User, CreateUserRequest, UpdateUserRequest } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl);
  }

  getById(id: string): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.baseUrl, request);
  }

  update(id: string, request: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  changePassword(id: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/change-password`, { newPassword });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

