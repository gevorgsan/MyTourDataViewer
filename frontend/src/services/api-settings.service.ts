import { api } from './api';
import type {
  ApiSettings,
  CreateApiSettingsRequest,
  UpdateApiSettingsRequest,
  TestConnectionRequest,
  TestConnectionResponse,
} from '../types/models';

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

function fromAuthorizationTypeValue(value: number | string | undefined): AuthorizationType {
  if (typeof value === 'string') {
    if (value === 'Bearer' || value === 'ApiKey' || value === 'Basic' || value === 'None') {
      return value;
    }
    return 'None';
  }
  switch (value) {
    case 1: return 'Bearer';
    case 2: return 'ApiKey';
    case 3: return 'Basic';
    default: return 'None';
  }
}

function toAuthorizationTypeValue(type: AuthorizationType): number {
  switch (type) {
    case 'Bearer': return 1;
    case 'ApiKey': return 2;
    case 'Basic': return 3;
    default: return 0;
  }
}

function parseCredentialsEmail(payload?: string): string | undefined {
  if (!payload) return undefined;
  try {
    const parsed = JSON.parse(payload) as Record<string, unknown>;
    return typeof parsed['email'] === 'string' ? parsed['email'] : undefined;
  } catch {
    return undefined;
  }
}

function buildCredentialsPayload(email?: string, password?: string): string | undefined {
  if (!email && !password) return undefined;
  const payload: Record<string, string> = {};
  if (email) payload['email'] = email;
  if (password) payload['password'] = password;
  return JSON.stringify(payload);
}

function resolveEndpointUrl(request: CreateApiSettingsRequest | UpdateApiSettingsRequest): string {
  const tokenUrl = request.tokenUrl?.trim();
  if (tokenUrl) return tokenUrl;
  const baseUrl = request.baseUrl?.trim();
  if (baseUrl) return baseUrl;
  return '/';
}

function buildEndpointName(name: string | undefined, endpointUrl: string): string {
  const normalizedName = name?.trim();
  if (normalizedName) return normalizedName;
  return `Authorization ${endpointUrl}`;
}

function buildEndpoints(
  request: CreateApiSettingsRequest | UpdateApiSettingsRequest,
  authorizationType: number,
): BackendApiEndpoint[] {
  const endpointUrl = resolveEndpointUrl(request);
  return [
    {
      name: buildEndpointName(request.name, endpointUrl),
      url: endpointUrl,
      httpMethod: 'GET',
      requiresAuthorization: authorizationType !== 0,
      authorizationType,
      tokenEndpointUrl: request.tokenUrl ?? undefined,
      username: request.credentialsEmail ?? undefined,
      password: request.credentialsPassword ?? undefined,
      headers: [],
    },
  ];
}

function mapFromBackend(item: BackendApiSettings): ApiSettings {
  const authorizationType = fromAuthorizationTypeValue(item.authorizationType);
  return {
    id: item.id,
    name: item.name,
    baseUrl: item.baseUrl,
    endpointUrls: '[]',
    authType: authorizationType,
    requiresAuthorization: authorizationType !== 'None',
    authorizationType,
    tokenUrl: item.tokenUrl,
    credentialsEmail: parseCredentialsEmail(item.credentialsPayload),
    timeoutSeconds: item.timeoutSeconds,
    isActive: item.isActive,
    createdAt: item.createdAt,
    updatedAt: item.updatedAt,
  };
}

function mapToBackend(
  request: CreateApiSettingsRequest | UpdateApiSettingsRequest,
): BackendApiSettingsUpsertRequest {
  const authorizationType = toAuthorizationTypeValue(request.authorizationType ?? 'None');
  return {
    name: request.name,
    baseUrl: request.baseUrl,
    endpoints: buildEndpoints(request, authorizationType),
    timeoutSeconds: request.timeoutSeconds,
    isActive: 'isActive' in request ? request.isActive : undefined,
    authorizationType,
    tokenUrl: request.tokenUrl,
    credentialsPayload: buildCredentialsPayload(request.credentialsEmail, request.credentialsPassword),
  };
}

export async function getAll(): Promise<ApiSettings[]> {
  const items = await api.get<BackendApiSettings[]>('/apisettings');
  return items.map(mapFromBackend);
}

export async function getById(id: number): Promise<ApiSettings> {
  const item = await api.get<BackendApiSettings>(`/apisettings/${id}`);
  return mapFromBackend(item);
}

export async function create(request: CreateApiSettingsRequest): Promise<ApiSettings> {
  const item = await api.post<BackendApiSettings>('/apisettings', mapToBackend(request));
  return mapFromBackend(item);
}

export function update(id: number, request: UpdateApiSettingsRequest): Promise<void> {
  return api.put<void>(`/apisettings/${id}`, mapToBackend(request));
}

export function deleteSettings(id: number): Promise<void> {
  return api.delete<void>(`/apisettings/${id}`);
}

export function testConnection(request: TestConnectionRequest): Promise<TestConnectionResponse> {
  return api.post<TestConnectionResponse>('/apisettings/test', request);
}
