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
  tokenUrl?: string;
  username?: string;
  password?: string;
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
    const primaryEndpoint = endpoints.find(endpoint => endpoint.authorizationType === 'Bearer') ?? endpoints[0];

    return {
      id: item.id,
      name: item.name,
      baseUrl: item.baseUrl,
      endpointUrls: '[]',
      authType: 'Bearer',
      requiresAuthorization: true,
      authorizationType: 'Bearer',
      tokenUrl: primaryEndpoint?.tokenEndpointUrl,
      username: primaryEndpoint?.username,
      password: undefined,
      apiKey: undefined,
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
    const endpointUrl = this.resolveEndpointUrl(request);

    return [{
      name: this.buildEndpointName(request.name, endpointUrl),
      url: endpointUrl,
      httpMethod: 'GET',
      requiresAuthorization: true,
      authorizationType: 'Bearer',
      tokenEndpointUrl: request.tokenUrl || undefined,
      username: request.username || undefined,
      password: request.password || undefined,
      headers: []
    }];
  }

  private resolveEndpointUrl(
    request: (CreateApiSettingsRequest | UpdateApiSettingsRequest) & ApiSettingsAuthorizationFields
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

