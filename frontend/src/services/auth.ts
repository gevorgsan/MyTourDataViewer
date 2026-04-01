import { API_BASE, ApiError, fetchWithRetry } from './api';
import type { LoginRequest, LoginResponse, User } from '../types/models';

const TOKEN_KEY = 'token';
const ROLE_CLAIM =
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function removeToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function isAuthenticated(): boolean {
  const token = getToken();
  if (!token) return false;
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return false;
    const payload = JSON.parse(atob(parts[1])) as Record<string, unknown>;
    return typeof payload['exp'] === 'number' && payload['exp'] * 1000 > Date.now();
  } catch {
    return false;
  }
}

export function getRoles(): string[] {
  const token = getToken();
  if (!token) return [];
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return [];
    const payload = JSON.parse(atob(parts[1])) as Record<string, unknown>;
    const roles = payload[ROLE_CLAIM];
    if (!roles) return [];
    return Array.isArray(roles) ? (roles as string[]) : [String(roles)];
  } catch {
    return [];
  }
}

export function isAdmin(): boolean {
  return getRoles().includes('Administrator');
}

/** Authenticate and store the JWT token. Throws ApiError on failure. */
export async function login(request: LoginRequest): Promise<LoginResponse> {
  const res = await fetchWithRetry(`${API_BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });

  let errorBody: unknown = null;
  if (!res.ok) {
    try {
      errorBody = await res.json();
    } catch {
      // ignore
    }
    const message =
      (errorBody as Record<string, string> | null)?.['message'] ??
      `HTTP ${res.status}`;
    throw new ApiError(message, res.status, errorBody);
  }

  const data = (await res.json()) as LoginResponse;
  setToken(data.token);
  return data;
}

/** Return current user profile. */
export async function me(): Promise<User> {
  const token = getToken();
  const res = await fetchWithRetry(`${API_BASE}/auth/me`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  if (!res.ok) {
    throw new ApiError(`HTTP ${res.status}`, res.status, null);
  }
  return res.json() as Promise<User>;
}
