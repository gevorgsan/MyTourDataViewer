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

interface ApiSettingsAuthorizationFields {
  requiresAuthorization?: boolean;
  authorizationType?: AuthorizationType;
  tokenUrl?: string;
  username?: string;
  password?: string;
  apiKey?: string;
  clientId?: string;
  clientSecret?: string;
}

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
  authorizationType: AuthorizationType;
  tokenEndpointUrl?: string;
  username?: string;
  password?: string;
  clientId?: string;
  clientSecret?: string;
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
}

interface BackendApiSettingsUpsertRequest {
  name?: string;
  baseUrl?: string;
  endpoints?: BackendApiEndpoint[];
  timeoutSeconds?: number;
  isActive?: boolean;
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

  create(request: CreateApiSettingsRequest & ApiSettingsAuthorizationFields): Observable<ApiSettings> {
    return this.http.post<BackendApiSettings>(this.baseUrl, this.mapToBackend(request)).pipe(
      map(item => this.mapFromBackend(item))
    );
  }

  update(id: number, request: UpdateApiSettingsRequest & ApiSettingsAuthorizationFields): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, this.mapToBackend(request));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  testConnection(request: TestConnectionRequest): Observable<TestConnectionResponse> {
    return this.http.post<TestConnectionResponse>(`${this.baseUrl}/test`, request);
  }

  private mapFromBackend(item: BackendApiSettings): ApiSettings {
    const endpoints = item.endpoints ?? [];
    const primaryEndpoint = endpoints[0];
    const authorizationType = primaryEndpoint?.authorizationType ?? 'None';
    const requiresAuthorization = primaryEndpoint?.requiresAuthorization ?? false;
    const apiKeyHeader = primaryEndpoint?.headers?.find(header =>
      header.name.toLowerCase() === 'x-api-key');

    return {
      id: item.id,
      name: item.name,
      baseUrl: item.baseUrl,
      endpointUrls: JSON.stringify(endpoints.map(endpoint => endpoint.url)),
      authType: requiresAuthorization ? authorizationType : 'None',
      requiresAuthorization,
      authorizationType,
      tokenUrl: primaryEndpoint?.tokenEndpointUrl,
      username: primaryEndpoint?.username,
      password: undefined,
      apiKey: apiKeyHeader?.value,
      clientId: primaryEndpoint?.clientId,
      timeoutSeconds: item.timeoutSeconds,
      isActive: item.isActive,
      createdAt: item.createdAt,
      updatedAt: item.updatedAt
    };
  }

  private mapToBackend(
    request: (CreateApiSettingsRequest | UpdateApiSettingsRequest) & ApiSettingsAuthorizationFields
  ): BackendApiSettingsUpsertRequest {
    return {
      name: request.name,
      baseUrl: request.baseUrl,
      endpoints: this.buildEndpoints(request),
      timeoutSeconds: request.timeoutSeconds,
      isActive: 'isActive' in request ? request.isActive : undefined
    };
  }

  private buildEndpoints(
    request: (CreateApiSettingsRequest | UpdateApiSettingsRequest) & ApiSettingsAuthorizationFields
  ): BackendApiEndpoint[] {
    const urls = this.parseEndpointUrls(request.endpointUrls);
    const requiresAuthorization = request.requiresAuthorization ?? false;
    const authorizationType = requiresAuthorization
      ? request.authorizationType ?? 'None'
      : 'None';

    return urls.map((url, index) => ({
      name: this.buildEndpointName(url, index),
      url,
      httpMethod: 'GET',
      requiresAuthorization,
      authorizationType,
      tokenEndpointUrl: authorizationType === 'Bearer' ? request.tokenUrl || undefined : undefined,
      username: authorizationType === 'Bearer' || authorizationType === 'Basic'
        ? request.username || undefined
        : undefined,
      password: authorizationType === 'Bearer' || authorizationType === 'Basic'
        ? request.password || undefined
        : undefined,
      clientId: authorizationType === 'Bearer' ? request.clientId || undefined : undefined,
      clientSecret: authorizationType === 'Bearer' ? request.clientSecret || undefined : undefined,
      headers: authorizationType === 'ApiKey' && request.apiKey
        ? [{ name: 'X-Api-Key', value: request.apiKey }]
        : []
    }));
  }

  private parseEndpointUrls(endpointUrls?: string): string[] {
    if (!endpointUrls) {
      return [];
    }

    try {
      const parsed = JSON.parse(endpointUrls) as unknown;
      if (Array.isArray(parsed)) {
        return parsed
          .filter((value): value is string => typeof value === 'string')
          .map(value => value.trim())
          .filter(Boolean);
      }
    } catch {
      const normalized = endpointUrls.trim();
      if (normalized) {
        return [normalized];
      }
    }

    return [];
  }

  private buildEndpointName(url: string, index: number): string {
    const normalized = url.trim();
    return normalized || `Endpoint ${index + 1}`;
  }
}

