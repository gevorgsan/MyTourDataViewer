const API_BASE = (import.meta.env.VITE_API_URL as string | undefined) ?? '/api';

/** Retrieve the stored JWT token from localStorage. */
function getStoredToken(): string | null {
  return localStorage.getItem('token');
}

export class ApiError extends Error {
  status: number;
  body: unknown;

  constructor(message: string, status: number, body: unknown) {
    super(message);
    this.status = status;
    this.body = body;
  }
}

async function request<T>(method: string, url: string, body?: unknown): Promise<T> {
  const token = getStoredToken();
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(url, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!res.ok) {
    let errorBody: unknown = null;
    try {
      errorBody = await res.json();
    } catch {
      // ignore parse errors
    }
    const message =
      (errorBody as Record<string, string> | null)?.['message'] ??
      (errorBody as Record<string, string> | null)?.['title'] ??
      `HTTP ${res.status}`;
    throw new ApiError(message, res.status, errorBody);
  }

  if (res.status === 204) {
    return undefined as T;
  }
  return res.json() as Promise<T>;
}

function buildUrl(path: string, params?: Record<string, string | number>): string {
  const url = `${API_BASE}${path}`;
  if (!params) return url;
  const qs = new URLSearchParams(
    Object.entries(params).map(([k, v]) => [k, String(v)]),
  ).toString();
  return `${url}?${qs}`;
}

export const api = {
  get: <T>(path: string, params?: Record<string, string | number>) =>
    request<T>('GET', buildUrl(path, params)),
  post: <T>(path: string, body?: unknown) =>
    request<T>('POST', buildUrl(path), body),
  put: <T>(path: string, body?: unknown) =>
    request<T>('PUT', buildUrl(path), body),
  delete: <T>(path: string) =>
    request<T>('DELETE', buildUrl(path)),
  /** Raw POST to a fully-qualified URL without base prefix (used for login retry). */
  postRaw: <T>(url: string, body?: unknown) =>
    request<T>('POST', url, body),
};

export { API_BASE };
