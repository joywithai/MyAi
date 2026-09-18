'use client';

import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { LogIn, LogOut, UserPlus } from 'lucide-react';
import clsx from 'clsx';
import { useApp } from '@/lib/app-context';

export function SiteHeader() {
  const { user, role, logout, locale, setLocale, t } = useApp();
  const pathname = usePathname();
  const router = useRouter();

  const navItems = user
    ? [
        { href: '/chat', label: t('nav.chat') },
        { href: '/settings', label: t('nav.settings') },
        { href: '/subscription', label: t('nav.subscription') },
        ...(role === 'admin' ? [{ href: '/admin', label: t('nav.admin') }] : [])
      ]
    : [];

  return (
    <header className="sticky top-0 z-40 border-b border-line bg-surface/90 backdrop-blur">
      <div className="mx-auto flex h-14 w-full max-w-page items-center justify-between px-4">
        <Link href={user ? '/chat' : '/'} className="flex items-center gap-2 font-bold">
          <span className="inline-block h-7 w-7 rounded-lg bg-brand text-center text-sm leading-7">AI</span>
          <span>{t('app.name')}</span>
        </Link>

        <nav className="flex items-center gap-1">
          {navItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={clsx(
                'rounded-lg px-3 py-1.5 text-sm transition',
                pathname?.startsWith(item.href)
                  ? 'bg-brand-soft text-brand-strong'
                  : 'text-gray-300 hover:bg-surface-raised'
              )}
            >
              {item.label}
            </Link>
          ))}
        </nav>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => setLocale(locale === 'bn' ? 'en' : 'bn')}
            className="rounded-lg border border-line px-2 py-1 text-xs text-gray-300 hover:border-brand"
            aria-label="Toggle language"
          >
            {locale === 'bn' ? 'বাংলা' : 'EN'}
          </button>

          {user ? (
            <button
              type="button"
              onClick={() => {
                logout();
                router.push('/');
              }}
              className="btn-secondary !py-1 text-xs"
            >
              <LogOut className="h-3.5 w-3.5" /> {t('nav.logout')}
            </button>
          ) : (
            <>
              <Link href="/login" className="btn-secondary !py-1 text-xs">
                <LogIn className="h-3.5 w-3.5" /> {t('nav.login')}
              </Link>
              <Link href="/register" className="btn-primary !py-1 text-xs">
                <UserPlus className="h-3.5 w-3.5" /> {t('nav.register')}
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
