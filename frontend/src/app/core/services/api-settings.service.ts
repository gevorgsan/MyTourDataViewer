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
    return this.http.get<ApiSettings[]>(this.baseUrl);
  }

  getById(id: number): Observable<ApiSettings> {
    return this.http.get<ApiSettings>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateApiSettingsRequest): Observable<ApiSettings> {
    return this.http.post<ApiSettings>(this.baseUrl, request);
  }

  update(id: number, request: UpdateApiSettingsRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  testConnection(request: TestConnectionRequest): Observable<TestConnectionResponse> {
    return this.http.post<TestConnectionResponse>(`${this.baseUrl}/test`, request);
  }
}

