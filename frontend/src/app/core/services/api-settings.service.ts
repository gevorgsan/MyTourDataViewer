import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ApiSettings,
  CreateApiSettingsRequest,
  UpdateApiSettingsRequest,
  TestConnectionRequest,
  TestConnectionResponse
} from '../models/models';
import { environment } from '../../../environments/environment';

type AuthorizationType = 'None' | 'Bearer' | 'ApiKey' | 'Basic';

interface BackendApiEndpointHeader {
  name: string;
  value: string;
}

interface BackendApiEndpoint {
  id?: number;
  name: string;
  url: string;
  httpMethod: string;
  requiresAuthorization: boolean;
  authorizationType: number;
  tokenEndpointUrl?: string;
  username?: string;
  password?: string;
  headers?: BackendApiEndpointHeader[];
}

interface BackendApiSettings {
  id: number;
  name: string;
  baseUrl: string;
  endpoints?: BackendApiEndpoint[];
  timeoutSeconds: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  authorizationType?: number | string;
  tokenUrl?: string;
  credentialsPayload?: string;
}

interface BackendApiSettingsUpsertRequest {
  name?: string;
  baseUrl?: string;
  endpoints?: BackendApiEndpoint[];
  timeoutSeconds?: number;
  isActive?: boolean;
  authorizationType?: number;
  tokenUrl?: string;
  credentialsPayload?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ApiSettingsService {
  private readonly baseUrl = `${environment.apiUrl}/apisettings`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiSettings[]> {
    return this.http.get<BackendApiSettings[]>(this.baseUrl).pipe(
      map(items => items.map(item => this.mapFromBackend(item)))
    );
  }

  getById(id: number): Observable<ApiSettings> {
    return this.http.get<BackendApiSettings>(`${this.baseUrl}/${id}`).pipe(
      map(item => this.mapFromBackend(item))
    );
  }

  create(request: CreateApiSettingsRequest): Observable<ApiSettings> {
    return this.http.post<BackendApiSettings>(this.baseUrl, this.mapToBackend(request)).pipe(
      map(item => this.mapFromBackend(item))
    );
  }

  update(id: number, request: UpdateApiSettingsRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, this.mapToBackend(request));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  testConnection(request: TestConnectionRequest): Observable<TestConnectionResponse> {
    return this.http.post<TestConnectionResponse>(`${this.baseUrl}/test`, request);
  }

  private mapFromBackend(item: BackendApiSettings): ApiSettings {
    const authorizationType = this.fromAuthorizationTypeValue(item.authorizationType);

    return {
      id: item.id,
      name: item.name,
      baseUrl: item.baseUrl,
      endpointUrls: '[]',
      authType: authorizationType,
      requiresAuthorization: authorizationType !== 'None',
      authorizationType,
      tokenUrl: item.tokenUrl,
      credentialsEmail: this.parseCredentialsEmail(item.credentialsPayload),
      timeoutSeconds: item.timeoutSeconds,
      isActive: item.isActive,
      createdAt: item.createdAt,
      updatedAt: item.updatedAt
    };
  }

  private parseCredentialsEmail(payload?: string): string | undefined {
    if (!payload) return undefined;
    try {
      const parsed = JSON.parse(payload);
      return typeof parsed?.email === 'string' ? parsed.email : undefined;
    } catch {
      return undefined;
    }
  }

  private mapToBackend(
    request: CreateApiSettingsRequest | UpdateApiSettingsRequest
  ): BackendApiSettingsUpsertRequest {
    const authorizationType = this.toAuthorizationTypeValue(request.authorizationType ?? 'None');

    const credentialsPayload = this.buildCredentialsPayload(
      request.credentialsEmail,
      request.credentialsPassword
    );

    return {
      name: request.name,
      baseUrl: request.baseUrl,
      endpoints: this.buildEndpoints(request, authorizationType),
      timeoutSeconds: request.timeoutSeconds,
      isActive: 'isActive' in request ? request.isActive : undefined,
      authorizationType,
      tokenUrl: request.tokenUrl,
      credentialsPayload
    };
  }

  private buildCredentialsPayload(email?: string, password?: string): string | undefined {
    if (!email && !password) return undefined;
    const payload: Record<string, string> = {};
    if (email) payload['email'] = email;
    if (password) payload['password'] = password;
    return JSON.stringify(payload);
  }

  private buildEndpoints(
    request: CreateApiSettingsRequest | UpdateApiSettingsRequest,
    authorizationType: number
  ): BackendApiEndpoint[] {
    const endpointUrl = this.resolveEndpointUrl(request);

    return [{
      name: this.buildEndpointName(request.name, endpointUrl),
      url: endpointUrl,
      httpMethod: 'GET',
      requiresAuthorization: authorizationType !== 0,
      authorizationType,
      tokenEndpointUrl: request.tokenUrl || undefined,
      username: request.credentialsEmail || undefined,
      password: request.credentialsPassword || undefined,
      headers: []
    }];
  }

  private toAuthorizationTypeValue(type: AuthorizationType): number {
    switch (type) {
      case 'Bearer':
        return 1;
      case 'ApiKey':
        return 2;
      case 'Basic':
        return 3;
      default:
        return 0;
    }
  }

  private fromAuthorizationTypeValue(value: number | string | undefined): AuthorizationType {
    if (typeof value === 'string') {
      if (value === 'Bearer' || value === 'ApiKey' || value === 'Basic' || value === 'None') {
        return value;
      }
      return 'None';
    }

    switch (value) {
      case 1:
        return 'Bearer';
      case 2:
        return 'ApiKey';
      case 3:
        return 'Basic';
      default:
        return 'None';
    }
  }

  private resolveEndpointUrl(
    request: CreateApiSettingsRequest | UpdateApiSettingsRequest
  ): string {
    const tokenUrl = request.tokenUrl?.trim();
    if (tokenUrl) {
      return tokenUrl;
    }

    const baseUrl = request.baseUrl?.trim();
    if (baseUrl) {
      return baseUrl;
    }

    return '/';
  }

  private buildEndpointName(name: string | undefined, endpointUrl: string): string {
    const normalizedName = name?.trim();
    if (normalizedName) {
      return normalizedName;
    }

    return `Authorization ${endpointUrl}`;
  }
}

