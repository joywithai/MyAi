'use client';

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { api, clearTokens, getAccessToken, setTokens } from './api';
import type { Locale } from './i18n';
import { translate } from './i18n';
import type { Role, TokenResponse, UserDto } from './types';

interface AppContextValue {
  user: UserDto | null;
  role: Role | null;
  loading: boolean;
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (key: string) => string;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => void;
  refreshUser: () => Promise<void>;
}

const AppContext = createContext<AppContextValue | null>(null);

export function AppProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [locale, setLocaleState] = useState<Locale>('bn');

  useEffect(() => {
    const stored = window.localStorage.getItem('myai.locale');
    if (stored === 'en' || stored === 'bn') setLocaleState(stored);
  }, []);

  const setLocale = useCallback((next: Locale) => {
    setLocaleState(next);
    window.localStorage.setItem('myai.locale', next);
    document.documentElement.lang = next;
  }, []);

  const t = useCallback((key: string) => translate(locale, key), [locale]);

  const refreshUser = useCallback(async () => {
    if (!getAccessToken()) {
      setUser(null);
      setLoading(false);
      return;
    }
    try {
      const profile = await api.get<{ user: UserDto }>('/user/profile');
      setUser(profile.user);
    } catch {
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refreshUser();
  }, [refreshUser]);

  const login = useCallback(async (email: string, password: string) => {
    const tokens = await api.postAnonymous<TokenResponse>('/auth/login', { email, password });
    setTokens(tokens.accessToken, tokens.refreshToken);
    setUser(tokens.user);
  }, []);

  const register = useCallback(async (email: string, password: string, displayName: string) => {
    const tokens = await api.postAnonymous<TokenResponse>('/auth/register', {
      email,
      password,
      displayName
    });
    setTokens(tokens.accessToken, tokens.refreshToken);
    setUser(tokens.user);
  }, []);

  const logout = useCallback(() => {
    const refreshToken = window.localStorage.getItem('myai.refreshToken');
    if (refreshToken) {
      void api.post('/auth/logout', { refreshToken }).catch(() => undefined);
    }
    clearTokens();
    setUser(null);
  }, []);

  const value = useMemo<AppContextValue>(
    () => ({
      user,
      role: user?.role ?? null,
      loading,
      locale,
      setLocale,
      t,
      login,
      register,
      logout,
      refreshUser
    }),
    [user, loading, locale, setLocale, t, login, register, logout, refreshUser]
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useApp(): AppContextValue {
  const context = useContext(AppContext);
  if (!context) throw new Error('useApp must be used within AppProvider');
  return context;
}
