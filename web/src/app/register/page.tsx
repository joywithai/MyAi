'use client';

// Registration — react-hook-form + zod schema validation (README §4.2).

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ApiError } from '@/lib/api';
import { useApp } from '@/lib/app-context';

const registerSchema = z.object({
  displayName: z.string().min(2, 'কমপক্ষে ২ অক্ষর / At least 2 characters').max(50),
  email: z.string().email('সঠিক ইমেইল দিন / Enter a valid email'),
  password: z
    .string()
    .min(8, 'কমপক্ষে ৮ অক্ষর / At least 8 characters')
    .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$/, 'বড় হাতের, ছোট হাতের ও সংখ্যা লাগবে / Need A-Z, a-z and 0-9')
});

type RegisterForm = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const { register: signUp, t } = useApp();
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<RegisterForm>({ resolver: zodResolver(registerSchema) });

  const onSubmit = async (values: RegisterForm) => {
    setError(null);
    try {
      await signUp(values.email, values.password, values.displayName);
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
    }
  };

  return (
    <div className="mx-auto max-w-sm py-14">
      <h1 className="mb-6 text-center text-2xl font-bold">{t('auth.register.title')}</h1>
      <form onSubmit={handleSubmit(onSubmit)} className="card space-y-4" noValidate>
        <div>
          <label className="label" htmlFor="displayName">{t('auth.displayName')}</label>
          <input id="displayName" className="input" {...register('displayName')} />
          {errors.displayName && (
            <p className="mt-1 text-xs text-danger">{errors.displayName.message}</p>
          )}
        </div>
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
            autoComplete="new-password"
            {...register('password')}
          />
          {errors.password ? (
            <p className="mt-1 text-xs text-danger">{errors.password.message}</p>
          ) : (
            <p className="mt-1 text-[11px] text-muted">8+ A a 1</p>
          )}
        </div>

        {error && <p className="rounded-lg bg-danger/10 px-3 py-2 text-xs text-danger">{error}</p>}

        <button type="submit" className="btn-primary w-full" disabled={isSubmitting}>
          {isSubmitting ? t('common.loading') : t('auth.submit.register')}
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
