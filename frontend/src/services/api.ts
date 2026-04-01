// VITE_API_BASE_URL is the backend origin (e.g. https://mytour-backend.onrender.com).
// Render's fromService host property returns a bare hostname; we prepend https:// when
// no scheme is present, then append /api.  Falls back to /api for the Vite dev-server
// proxy (see vite.config.ts) when the variable is not set.
function resolveApiBase(): string {
  const raw = import.meta.env.VITE_API_BASE_URL as string | undefined;
  if (!raw) return '/api';
  const withScheme =
    raw.startsWith('http://') || raw.startsWith('https://') ? raw : `https://${raw}`;
  return `${withScheme.replace(/\/$/, '')}/api`;
}

const API_BASE = resolveApiBase();

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

// ── Retry logic for Render free-tier cold starts ────────────────────────────
// Render's free-tier services sleep after 15 minutes of inactivity.  When the
// first request arrives, Render's load balancer may immediately return 502/503
// before the backend has finished waking up.  We retry these gateway errors with
// exponential back-off so the user doesn't have to manually refresh.
const RETRYABLE_STATUSES = new Set([502, 503, 504]);
const MAX_RETRIES = 3;
const BASE_DELAY_MS = 2_000; // 2 s → 4 s → 8 s

export async function fetchWithRetry(
  input: RequestInfo | URL,
  init?: RequestInit,
  retries = MAX_RETRIES,
): Promise<Response> {
  for (let attempt = 0; ; attempt++) {
    const res = await fetch(input, init);
    if (!RETRYABLE_STATUSES.has(res.status) || attempt >= retries) {
      return res;
    }
    await new Promise((resolve) => setTimeout(resolve, BASE_DELAY_MS * 2 ** attempt));
  }
}

async function request<T>(method: string, url: string, body?: unknown): Promise<T> {
  const token = getStoredToken();
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetchWithRetry(url, {
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
