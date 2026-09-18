'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { ApiError } from '@/lib/api';
import { useApp } from '@/lib/app-context';

export default function LoginPage() {
  const { login, t } = useApp();
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      await login(email, password);
      router.push('/chat');
    } catch (err) {
      setError(
        err instanceof ApiError && err.status === 401
          ? 'ভুল ইমেইল বা পাসওয়ার্ড / Wrong email or password'
          : t('common.error')
      );
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="mx-auto max-w-sm py-14">
      <h1 className="mb-6 text-center text-2xl font-bold">{t('auth.login.title')}</h1>
      <form onSubmit={handleSubmit} className="card space-y-4">
        <div>
          <label className="label" htmlFor="email">{t('auth.email')}</label>
          <input
            id="email"
            type="email"
            className="input"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            autoComplete="email"
          />
        </div>
        <div>
          <label className="label" htmlFor="password">{t('auth.password')}</label>
          <input
            id="password"
            type="password"
            className="input"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="current-password"
          />
        </div>

        {error && <p className="rounded-lg bg-danger/10 px-3 py-2 text-xs text-danger">{error}</p>}

        <button type="submit" className="btn-primary w-full" disabled={busy}>
          {busy ? t('common.loading') : t('auth.submit.login')}
        </button>

        <p className="text-center text-xs text-muted">
          {t('auth.noAccount')}{' '}
          <Link href="/register" className="text-brand-strong hover:underline">
            {t('auth.submit.register')}
          </Link>
        </p>
      </form>
    </div>
  );
}
