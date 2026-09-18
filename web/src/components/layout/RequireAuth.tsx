'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useApp } from '@/lib/app-context';
import type { Role } from '@/lib/types';

export function RequireAuth({
  children,
  minRole
}: {
  children: React.ReactNode;
  minRole?: Role;
}) {
  const { user, loading } = useApp();
  const router = useRouter();

  useEffect(() => {
    if (!loading && !user) router.replace('/login');
  }, [loading, user, router]);

  if (loading) {
    return <div className="py-24 text-center text-muted">Loading…</div>;
  }

  if (!user) return null;

  if (minRole === 'admin' && user.role !== 'admin') {
    return <div className="py-24 text-center text-danger">403 — admin only</div>;
  }

  return <>{children}</>;
}
