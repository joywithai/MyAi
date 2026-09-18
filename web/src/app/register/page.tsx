'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { ApiError } from '@/lib/api';
import { useApp } from '@/lib/app-context';

export default function RegisterPage() {
  const { register, t } = useApp();
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      await register(email, password, displayName);
      router.push('/chat');
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 409) {
          setError('এই ইমেইল ইতিমধ্যে ব্যবহৃত / Email already in use');
        } else if (err.body.errors) {
          setError(Object.values(err.body.errors).flat().join(' · '));
        } else {
          setError(t('common.error'));
        }
      } else {
        setError(t('common.error'));
      }
    } finally {
      setBusy(false);
    }
  };

  const passwordRuleOk = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/.test(password);

  return (
    <div className="mx-auto max-w-sm py-14">
      <h1 className="mb-6 text-center text-2xl font-bold">{t('auth.register.title')}</h1>
      <form onSubmit={handleSubmit} className="card space-y-4">
        <div>
          <label className="label" htmlFor="displayName">{t('auth.displayName')}</label>
          <input
            id="displayName"
            className="input"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            required
            maxLength={50}
          />
        </div>
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
            autoComplete="new-password"
          />
          <p className={`mt-1 text-[11px] ${passwordRuleOk ? 'text-accent' : 'text-muted'}`}>
            8+ A a 1 — {passwordRuleOk ? '✓' : '…'}
          </p>
        </div>

        {error && <p className="rounded-lg bg-danger/10 px-3 py-2 text-xs text-danger">{error}</p>}

        <button type="submit" className="btn-primary w-full" disabled={busy}>
          {busy ? t('common.loading') : t('auth.submit.register')}
        </button>

        <p className="text-center text-xs text-muted">
          {t('auth.hasAccount')}{' '}
          <Link href="/login" className="text-brand-strong hover:underline">
            {t('auth.submit.login')}
          </Link>
        </p>
      </form>
    </div>
  );
}
