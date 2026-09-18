'use client';

// Demo payment flow: initiate → process → role upgrade. Real Stripe /
// SSLCommerz redirects are future-ready on the backend.

import { useCallback, useEffect, useState } from 'react';
import { RequireAuth } from '@/components/layout/RequireAuth';
import { useApp } from '@/lib/app-context';
import { api, ApiError } from '@/lib/api';
import type { CheckoutData, SubscriptionDto, SubscriptionPlanDto } from '@/lib/types';

function SubscriptionInner() {
  const { t, locale, refreshUser } = useApp();
  const [plans, setPlans] = useState<SubscriptionPlanDto[]>([]);
  const [current, setCurrent] = useState<SubscriptionDto | null>(null);
  const [busyPlanId, setBusyPlanId] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setPlans(await api.get<SubscriptionPlanDto[]>('/subscriptions/plans'));
    } catch {
      /* noop */
    }
    try {
      setCurrent(await api.get<SubscriptionDto | null>('/subscriptions/current'));
    } catch {
      /* noop */
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const pay = async (planId: string) => {
    setBusyPlanId(planId);
    setError(null);
    setMessage(null);
    try {
      const checkout = await api.post<CheckoutData>('/payments/initiate', { planId });

      if (!checkout.isDemo && checkout.checkoutUrl) {
        window.location.href = checkout.checkoutUrl;
        return;
      }

      // Demo provider: complete immediately.
      await api.post('/payments/process', {
        transactionId: checkout.transactionId,
        providerTransactionId: checkout.sessionData,
        providerData: { demo: true }
      });

      setMessage(
        locale === 'bn'
          ? '🎉 পেমেন্ট সফল! আপনি এখন সাবস্ক্রাইবার।'
          : '🎉 Payment successful! You are now a subscriber.'
      );
      await load();
      await refreshUser();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'));
    } finally {
      setBusyPlanId(null);
    }
  };

  return (
    <div className="space-y-6 py-2">
      <h1 className="text-2xl font-bold">{t('subscription.title')}</h1>

      <div className="card">
        <h2 className="text-sm font-semibold text-muted">{t('subscription.current')}</h2>
        {current ? (
          <p className="mt-1 text-lg font-semibold text-accent">
            {current.planName ?? 'Premium'} · {current.status} ·{' '}
            {new Date(current.endDate).toLocaleDateString(locale === 'bn' ? 'bn-BD' : 'en-US')}
          </p>
        ) : (
          <p className="mt-1 text-lg">{t('subscription.none')}</p>
        )}
      </div>

      {message && <div className="rounded-lg bg-accent/10 px-4 py-3 text-sm text-accent">{message}</div>}
      {error && <div className="rounded-lg bg-danger/10 px-4 py-3 text-sm text-danger">{error}</div>}

      <div className="grid gap-4 sm:grid-cols-2">
        {plans.map((plan) => (
          <div
            key={plan.id}
            className={`card flex flex-col ${plan.billingCycle === 'Yearly' ? 'border-brand/50' : ''}`}
          >
            <div className="mb-2 flex items-center justify-between">
              <h3 className="text-lg font-bold">{plan.name}</h3>
              <span className="rounded-full bg-brand-soft px-2 py-0.5 text-[11px] text-brand-strong">
                {plan.billingCycle === 'Yearly' ? t('subscription.yearly') : t('subscription.monthly')}
              </span>
            </div>
            <p className="text-sm text-muted">{plan.description}</p>
            <p className="my-3 text-3xl font-black">
              ৳{plan.price.toLocaleString()}
              <span className="text-sm font-normal text-muted">
                /{plan.billingCycle === 'Yearly' ? (locale === 'bn' ? 'বছর' : 'yr') : locale === 'bn' ? 'মাস' : 'mo'}
              </span>
            </p>
            <ul className="mb-5 flex-1 space-y-1 text-sm text-muted">
              {(plan.features ?? []).map((feature) => (
                <li key={feature}>✅ {feature}</li>
              ))}
            </ul>
            <button
              type="button"
              className="btn-primary"
              disabled={busyPlanId === plan.id}
              onClick={() => void pay(plan.id)}
            >
              {busyPlanId === plan.id ? t('common.loading') : t('subscription.pay')}
            </button>
          </div>
        ))}
      </div>

      <p className="text-center text-xs text-muted">ℹ️ {t('subscription.demoNote')}</p>
    </div>
  );
}

export default function SubscriptionPage() {
  return (
    <RequireAuth>
      <SubscriptionInner />
    </RequireAuth>
  );
}
