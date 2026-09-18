'use client';

// Login form — react-hook-form + zod (README §4.2 form & validation stack).

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ApiError } from '@/lib/api';
import { useApp } from '@/lib/app-context';

const loginSchema = z.object({
  email: z.string().email('সঠিক ইমেইল দিন / Enter a valid email'),
  password: z.string().min(1, 'পাসওয়ার্ড দিন / Password required')
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const { login, t } = useApp();
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) });

  const onSubmit = async (values: LoginForm) => {
    setError(null);
    try {
      await login(values.email, values.password);
      router.push('/chat');
    } catch (err) {
      setError(
        err instanceof ApiError && err.status === 401
          ? 'ভুল ইমেইল বা পাসওয়ার্ড / Wrong email or password'
          : t('common.error')
      );
    }
  };

  return (
    <div className="mx-auto max-w-sm py-14">
      <h1 className="mb-6 text-center text-2xl font-bold">{t('auth.login.title')}</h1>
      <form onSubmit={handleSubmit(onSubmit)} className="card space-y-4" noValidate>
        <div>
          <label className="label" htmlFor="email">{t('auth.email')}</label>
          <input
            id="email"
            type="email"
            className="input"
            autoComplete="email"
            {...register('email')}
          />
          {errors.email && <p className="mt-1 text-xs text-danger">{errors.email.message}</p>}
        </div>
        <div>
          <label className="label" htmlFor="password">{t('auth.password')}</label>
          <input
            id="password"
            type="password"
            className="input"
            autoComplete="current-password"
            {...register('password')}
          />
          {errors.password && <p className="mt-1 text-xs text-danger">{errors.password.message}</p>}
        </div>

        {error && <p className="rounded-lg bg-danger/10 px-3 py-2 text-xs text-danger">{error}</p>}

        <button type="submit" className="btn-primary w-full" disabled={isSubmitting}>
          {isSubmitting ? t('common.loading') : t('auth.submit.login')}
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
