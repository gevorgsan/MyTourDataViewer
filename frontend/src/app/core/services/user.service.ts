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
    // TODO: implement
    throw new Error('Not implemented');
  }

  getById(id: string): Observable<User> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  create(request: CreateUserRequest): Observable<User> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  update(id: string, request: UpdateUserRequest): Observable<void> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  delete(id: string): Observable<void> {
    // TODO: implement
    throw new Error('Not implemented');
  }
}

