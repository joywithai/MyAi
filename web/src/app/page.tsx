'use client';

import Link from 'next/link';
import { Bot, Brain, Volume2 } from 'lucide-react';
import { useApp } from '@/lib/app-context';

export default function LandingPage() {
  const { t, user, locale } = useApp();

  const features = [
    { Icon: Brain, title: t('landing.features.ai'), description: t('landing.features.ai.desc') },
    { Icon: Bot, title: t('landing.features.avatar'), description: t('landing.features.avatar.desc') },
    { Icon: Volume2, title: t('landing.features.voice'), description: t('landing.features.voice.desc') }
  ];

  return (
    <div className="flex flex-col items-center py-14 text-center">
      <div className="mb-6 flex h-20 w-20 items-center justify-center rounded-3xl bg-gradient-to-br from-brand to-accent text-3xl font-black text-white shadow-[0_0_60px_-10px] shadow-brand">
        AI
      </div>

      <h1 className="text-4xl font-black tracking-tight sm:text-5xl">
        {t('app.name')} — {t('app.tagline')}
      </h1>
      <p className="mt-4 max-w-xl text-muted">
        {locale === 'bn'
          ? 'একটি 3D VRM অ্যাভাটার, যে বাংলা ও ইংরেজিতে কথা বলে, অভিব্যক্তি দেখায় এবং লাইপ-সিঙ্কে আপনার সাথে কথোপকথন করে।'
          : 'A 3D VRM avatar that talks in Bangla and English, shows expressions and lip-syncs while chatting with you.'}
      </p>

      <div className="mt-8 flex gap-3">
        <Link href={user ? '/chat' : '/register'} className="btn-primary !px-6 !py-2.5">
          {t('landing.cta')} →
        </Link>
        {!user && (
          <Link href="/login" className="btn-secondary !px-6 !py-2.5">
            {t('nav.login')}
          </Link>
        )}
      </div>

      <div className="mt-16 grid w-full gap-4 sm:grid-cols-3">
        {features.map((feature) => (
          <div key={feature.title} className="card text-left">
            <feature.Icon className="mb-3 h-8 w-8 text-brand-strong" />
            <h3 className="mb-1 font-semibold">{feature.title}</h3>
            <p className="text-sm text-muted">{feature.description}</p>
          </div>
        ))}
      </div>

      <div className="mt-12 grid w-full gap-4 text-left sm:grid-cols-2">
        <div className="card">
          <h3 className="mb-2 font-semibold">
            {locale === 'bn' ? 'ফ্রি ইউজার পান' : 'Free users get'}
          </h3>
          <ul className="space-y-1 text-sm text-muted">
            <li>✅ {locale === 'bn' ? '৪টি অভিব্যক্তি' : '4 expressions'}</li>
            <li>✅ {locale === 'bn' ? '১০টি সংরক্ষিত কথোপকথন' : '10 saved conversations'}</li>
            <li>✅ {locale === 'bn' ? 'দিনে ৫০টি বার্তা' : '50 messages/day'}</li>
            <li>✅ {locale === 'bn' ? 'ব্রিদিং অ্যানিমেশন' : 'Breathing animation'}</li>
          </ul>
        </div>
        <div className="card border-brand/40">
          <h3 className="mb-2 font-semibold">
            {locale === 'bn' ? 'সাবস্ক্রাইবার পান' : 'Subscribers get'}
          </h3>
          <ul className="space-y-1 text-sm text-muted">
            <li>✅ {locale === 'bn' ? '১২টি অভিব্যক্তি + সব অ্যানিমেশন' : '12 expressions + all animations'}</li>
            <li>✅ {locale === 'bn' ? 'নিজের OpenRouter API কী' : 'Your own OpenRouter API key'}</li>
            <li>✅ {locale === 'bn' ? 'দিনে ৫০০টি বার্তা' : '500 messages/day'}</li>
            <li>✅ {locale === 'bn' ? 'সম্পূর্ণ হিস্ট্রি ও অ্যাভাটার নির্বাচন' : 'Full history & avatar selection'}</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
