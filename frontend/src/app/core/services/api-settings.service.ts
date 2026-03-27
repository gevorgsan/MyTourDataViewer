import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApiSettings,
  CreateApiSettingsRequest,
  UpdateApiSettingsRequest,
  TestConnectionRequest,
  TestConnectionResponse
} from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiSettingsService {
  private readonly baseUrl = `${environment.apiUrl}/apisettings`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiSettings[]> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  getById(id: number): Observable<ApiSettings> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  create(request: CreateApiSettingsRequest): Observable<ApiSettings> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  update(id: number, request: UpdateApiSettingsRequest): Observable<void> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  delete(id: number): Observable<void> {
    // TODO: implement
    throw new Error('Not implemented');
  }

  testConnection(request: TestConnectionRequest): Observable<TestConnectionResponse> {
    // TODO: implement
    throw new Error('Not implemented');
  }
}

