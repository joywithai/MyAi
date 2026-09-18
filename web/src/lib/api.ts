import axios, { AxiosError, type AxiosInstance } from 'axios';
import type { ApiErrorBody } from './types';

export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:8080/api/v1';

const ACCESS_TOKEN_KEY = 'myai.accessToken';
const REFRESH_TOKEN_KEY = 'myai.refreshToken';

export function getAccessToken(): string | null {
  if (typeof window === 'undefined') return null;
  return window.localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function setTokens(accessToken: string, refreshToken: string): void {
  window.localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  window.localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

export function clearTokens(): void {
  window.localStorage.removeItem(ACCESS_TOKEN_KEY);
  window.localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export class ApiError extends Error {
  readonly status: number;

  readonly body: ApiErrorBody;

  constructor(status: number, body: ApiErrorBody) {
    super(body.message ?? body.error ?? `Request failed (${status})`);
    this.status = status;
    this.body = body;
  }
}

/** Shared axios instance (README §4.2: axios as HTTP client). */
const http: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' }
});

let refreshingPromise: Promise<boolean> | null = null;

async function tryRefresh(): Promise<boolean> {
  if (!refreshingPromise) {
    refreshingPromise = (async () => {
      const refreshToken = window.localStorage.getItem(REFRESH_TOKEN_KEY);
      if (!refreshToken) return false;

      try {
        const response = await http.post<{ accessToken: string; refreshToken: string }>(
          '/auth/refresh',
          { refreshToken }
        );
        setTokens(response.data.accessToken, response.data.refreshToken);
        return true;
      } catch {
        return false;
      } finally {
        refreshingPromise = null;
      }
    })();
  }
  return refreshingPromise;
}

interface RequestOptions {
  method?: string;
  body?: unknown;
  auth?: boolean;
  signal?: AbortSignal;
}

async function request<T>(
  path: string,
  { method = 'GET', body, auth = true, signal }: RequestOptions = {},
  retried = false
): Promise<T> {
  const headers: Record<string, string> = {};

  if (auth) {
    const token = getAccessToken();
    if (token) headers.Authorization = `Bearer ${token}`;
  }

  try {
    const response = await http.request<T>({
      url: path,
      method,
      data: body === undefined ? undefined : JSON.stringify(body),
      headers,
      signal
    });
    return response.data === ('' as unknown as T) ? (undefined as T) : response.data;
  } catch (error) {
    const axiosError = error as AxiosError<ApiErrorBody>;

    if (axiosError.response?.status === 401 && auth && !retried) {
      const refreshed = await tryRefresh();
      if (refreshed) {
        return request<T>(path, { method, body, auth, signal }, true);
      }
      clearTokens();
    }

    throw new ApiError(
      axiosError.response?.status ?? 0,
      axiosError.response?.data ?? {}
    );
  }
}

export const api = {
  get: <T>(path: string, signal?: AbortSignal) => request<T>(path, { signal }),
  post: <T>(path: string, body?: unknown, signal?: AbortSignal) =>
    request<T>(path, { method: 'POST', body, signal }),
  put: <T>(path: string, body?: unknown, signal?: AbortSignal) =>
    request<T>(path, { method: 'PUT', body, signal }),
  delete: <T>(path: string, signal?: AbortSignal) =>
    request<T>(path, { method: 'DELETE', signal }),
  postAnonymous: <T>(path: string, body?: unknown, signal?: AbortSignal) =>
    request<T>(path, { method: 'POST', body, auth: false, signal })
};

/** Query-string helper. */
export function qs(params: Record<string, string | number | undefined>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined) search.set(key, String(value));
  }
  const encoded = search.toString();
  return encoded ? `?${encoded}` : '';
}
