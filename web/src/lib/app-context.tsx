'use client';

// Global app state backed by Zustand (README §4.2 state management).
// AppProvider only bootstraps persisted state on mount.

import { useEffect } from 'react';
import { create } from 'zustand';
import { api, clearTokens, getAccessToken, setTokens } from './api';
import type { Locale } from './i18n';
import { translate } from './i18n';
import type { Role, TokenResponse, UserDto } from './types';

interface AppStore {
  user: UserDto | null;
  loading: boolean;
  locale: Locale;
  role: Role | null;
  hydrated: boolean;
  setLocale: (locale: Locale) => void;
  t: (key: string) => string;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => void;
  refreshUser: () => Promise<void>;
}

export const useAppStore = create<AppStore>()((set, get) => ({
  user: null,
  loading: true,
  locale: 'bn',
  role: null,
  hydrated: false,

  setLocale: (locale) => {
    set({ locale });
    window.localStorage.setItem('myai.locale', locale);
    document.documentElement.lang = locale;
  },

  t: (key) => translate(get().locale, key),

  login: async (email, password) => {
    const tokens = await api.postAnonymous<TokenResponse>('/auth/login', { email, password });
    setTokens(tokens.accessToken, tokens.refreshToken);
    set({ user: tokens.user, role: tokens.user.role });
  },

  register: async (email, password, displayName) => {
    const tokens = await api.postAnonymous<TokenResponse>('/auth/register', {
      email,
      password,
      displayName
    });
    setTokens(tokens.accessToken, tokens.refreshToken);
    set({ user: tokens.user, role: tokens.user.role });
  },

  logout: () => {
    const refreshToken = window.localStorage.getItem('myai.refreshToken');
    if (refreshToken) {
      void api.post('/auth/logout', { refreshToken }).catch(() => undefined);
    }
    clearTokens();
    set({ user: null, role: null });
  },

  refreshUser: async () => {
    if (!getAccessToken()) {
      set({ user: null, role: null, loading: false });
      return;
    }
    try {
      const profile = await api.get<{ user: UserDto }>('/user/profile');
      set({ user: profile.user, role: profile.user.role, loading: false });
    } catch {
      set({ user: null, role: null, loading: false });
    }
  }
}));

/** Bootstraps locale + session once on app start. */
export function AppProvider({ children }: { children: React.ReactNode }) {
  const refreshUser = useAppStore((state) => state.refreshUser);
  const setLocale = useAppStore((state) => state.setLocale);
  const hydrated = useAppStore((state) => state.hydrated);

  useEffect(() => {
    if (hydrated) return;
    const stored = window.localStorage.getItem('myai.locale');
    if (stored === 'en' || stored === 'bn') {
      setLocale(stored);
      document.documentElement.lang = stored;
    }
    void refreshUser();
    useAppStore.setState({ hydrated: true });
  }, [hydrated, refreshUser, setLocale]);

  return <>{children}</>;
}

/** Same hook API as before — components don't change. */
export function useApp(): AppStore {
  return useAppStore();
}
