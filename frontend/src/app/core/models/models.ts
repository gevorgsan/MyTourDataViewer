export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  fullName: string;
  roles: string[];
  expiry: string;
}

export interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  isActive: boolean;
  createdAt: string;
  roles: string[];
}

export interface CreateUserRequest {
  username: string;
  email: string;
  fullName: string;
  password: string;
  role: string;
}

export interface UpdateUserRequest {
  fullName?: string;
  email?: string;
  role?: string;
  isActive?: boolean;
  password?: string;
}

export interface ApiSettings {
  id: number;
  name: string;
  baseUrl: string;
  endpointUrls: string;
  authType: string;
  requiresAuthorization?: boolean;
  authorizationType: 'None' | 'Bearer' | 'ApiKey' | 'Basic';
  tokenUrl?: string;
  credentialsEmail?: string;
  timeoutSeconds: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateApiSettingsRequest {
  name: string;
  baseUrl?: string;
  endpointUrls?: string;
  authorizationType?: 'None' | 'Bearer' | 'ApiKey' | 'Basic';
  tokenUrl?: string;
  credentialsEmail?: string;
  credentialsPassword?: string;
  timeoutSeconds?: number;
}

export interface UpdateApiSettingsRequest {
  name?: string;
  baseUrl?: string;
  endpointUrls?: string;
  authorizationType?: 'None' | 'Bearer' | 'ApiKey' | 'Basic';
  tokenUrl?: string;
  credentialsEmail?: string;
  credentialsPassword?: string;
  timeoutSeconds?: number;
  isActive?: boolean;
}

export interface AvailableApi {
  id: number;
  name: string;
  baseUrl: string;
  endpointUrls: string;
}

export interface SearchRequestRequest {
  createdFrom: string;
  createdTo: string;
  requestChanels?: number | null;
  requestStatus?: number | null;
}

export interface SearchRequestItem {
  id: number;
  offerId?: number;
  price?: number;
  startDate?: string;
  endDate?: string;
  travelers?: unknown[];
  status?: number;
  currency?: string;
  createdDate?: string;
}

export interface RequestHistoryItem {
  requestId: number;
  changeType?: string;
  changedAt: string;
  changedBy?: string;
  oldValues?: Record<string, string | null>;
  newValues?: Record<string, string | null>;
}

export interface TestConnectionRequest {
  apiSettingsId: number;
  endpointPath?: string;
}

export interface TestConnectionResponse {
  success: boolean;
  statusCode?: number;
  message?: string;
  responseBody?: string;
}
