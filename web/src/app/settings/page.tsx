'use client';

// Settings hub: profile, voice, expressions, animations, avatar, custom AI key.
// Subscriber-locked controls render disabled with a 🔒 hint (§2.2 matrix).

import { useCallback, useEffect, useState } from 'react';
import { RequireAuth } from '@/components/layout/RequireAuth';
import { useApp } from '@/lib/app-context';
import { api, ApiError } from '@/lib/api';
import { normalizeExpression } from '@/lib/avatar/expressionMap';
import type {
  AnimationDto,
  AvatarModelDto,
  CustomAiConfigDto,
  ExpressionDto,
  FeatureFlagsDto,
  Language,
  UserSettingsDto
} from '@/lib/types';

const VOICE_OPTIONS = [
  { value: 'bn-BD-NabanitaNeural', label: 'নবনীতা (বাংলা নারী)' },
  { value: 'bn-BD-PradeepNeural', label: 'প্রদীপ (বাংলা পুরুষ)' },
  { value: 'en-US-JennyNeural', label: 'Jenny (English female)' },
  { value: 'en-US-GuyNeural', label: 'Guy (English male)' }
];

function SettingsInner() {
  const { user, t, locale } = useApp();
  const [settings, setSettings] = useState<UserSettingsDto | null>(null);
  const [flags, setFlags] = useState<FeatureFlagsDto | null>(null);
  const [expressions, setExpressions] = useState<ExpressionDto[]>([]);
  const [animations, setAnimations] = useState<AnimationDto[]>([]);
  const [avatars, setAvatars] = useState<AvatarModelDto[]>([]);
  const [customAi, setCustomAi] = useState<CustomAiConfigDto | null>(null);
  const [apiKey, setApiKey] = useState('');
  const [preferredModel, setPreferredModel] = useState('google/gemini-2.0-flash-001:free');
  const [displayName, setDisplayName] = useState('');
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void (async () => {
      try {
        setSettings(await api.get<UserSettingsDto>('/settings'));
      } catch {
        /* noop */
      }
      try {
        setFlags(await api.get<FeatureFlagsDto>('/settings/feature-flags'));
      } catch {
        /* noop */
      }
      try {
        setExpressions(await api.get<ExpressionDto[]>('/expressions'));
      } catch {
        /* noop */
      }
      try {
        setAnimations(await api.get<AnimationDto[]>('/animations'));
      } catch {
        /* noop */
      }
      try {
        setAvatars(await api.get<AvatarModelDto[]>('/avatars/models'));
      } catch {
        /* noop */
      }
      try {
        setCustomAi(await api.get<CustomAiConfigDto>('/custom-ai'));
        if (customAi?.preferredModel) setPreferredModel(customAi.preferredModel);
      } catch {
        /* noop */
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const patch = useCallback(
    async (partial: Partial<UserSettingsDto> & { clearAvatarModel?: boolean }) => {
      setError(null);
      setMessage(null);
      try {
        const updated = await api.put<UserSettingsDto>('/settings', partial);
        setSettings(updated);
        setMessage(t('settings.saved'));
      } catch (err) {
        setError(err instanceof ApiError ? err.message : t('common.error'));
      }
    },
    [t]
  );

  const flash = (text: string) => {
    setMessage(text);
    setTimeout(() => setMessage(null), 2500);
  };

  if (!settings || !flags) {
    return <div className="py-20 text-center text-muted">{t('common.loading')}</div>;
  }

  return (
    <div className="space-y-6 py-2">
      <h1 className="text-2xl font-bold">{t('settings.title')}</h1>
      {message && <div className="rounded-lg bg-accent/10 px-3 py-2 text-sm text-accent">{message}</div>}
      {error && <div className="rounded-lg bg-danger/10 px-3 py-2 text-sm text-danger">{error}</div>}

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Profile */}
        <section className="card">
          <h2 className="mb-4 font-semibold">👤 {t('settings.profile')}</h2>
          <div className="space-y-3">
            <div>
              <label className="label">{t('auth.email')}</label>
              <input className="input" value={user?.email ?? ''} disabled />
            </div>
            <div>
              <label className="label">{t('auth.displayName')}</label>
              <div className="flex gap-2">
                <input
                  className="input"
                  value={displayName}
                  placeholder={user?.displayName ?? ''}
                  onChange={(event) => setDisplayName(event.target.value)}
                />
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={async () => {
                    if (!displayName.trim()) return;
                    await api.put('/user/profile', { displayName: displayName.trim() });
                    setDisplayName('');
                    flash(t('settings.saved'));
                  }}
                >
                  {t('settings.save')}
                </button>
              </div>
            </div>
            <div>
              <label className="label">{t('settings.language')}</label>
              <div className="flex gap-2">
                {(['bn', 'en'] as Language[]).map((code) => (
                  <button
                    key={code}
                    type="button"
                    onClick={() => void patch({ preferredLanguage: code })}
                    className={`rounded-lg border px-3 py-1.5 text-sm ${
                      settings.preferredLanguage === code
                        ? 'border-brand bg-brand-soft text-brand-strong'
                        : 'border-line text-gray-300'
                    }`}
                  >
                    {code === 'bn' ? 'বাংলা' : 'English'}
                  </button>
                ))}
              </div>
            </div>
          </div>
        </section>

        {/* Voice */}
        <section className="card">
          <h2 className="mb-4 font-semibold">
            🎙 {t('settings.voice')} {!flags.canCustomizeVoice && '🔒'}
          </h2>
          <div className="space-y-4">
            <div>
              <label className="label">Voice</label>
              <select
                className="input"
                value={settings.voiceName}
                disabled={!flags.canCustomizeVoice}
                onChange={(event) => void patch({ voiceName: event.target.value })}
              >
                {VOICE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
            <label className="block text-xs text-muted">
              Speed: {settings.voiceSpeed.toFixed(2)}×
              <input
                type="range"
                className="mt-1 w-full"
                min={0.5}
                max={2}
                step={0.05}
                value={settings.voiceSpeed}
                disabled={!flags.canCustomizeVoice}
                onChange={(event) =>
                  setSettings({ ...settings, voiceSpeed: Number(event.target.value) })
                }
                onMouseUp={() => void patch({ voiceSpeed: settings.voiceSpeed })}
                onTouchEnd={() => void patch({ voiceSpeed: settings.voiceSpeed })}
              />
            </label>
            <label className="block text-xs text-muted">
              Pitch: {settings.voicePitch}
              <input
                type="range"
                className="mt-1 w-full"
                min={-50}
                max={50}
                step={5}
                value={settings.voicePitch}
                disabled={!flags.canCustomizeVoice}
                onChange={(event) =>
                  setSettings({ ...settings, voicePitch: Number(event.target.value) })
                }
                onMouseUp={() => void patch({ voicePitch: settings.voicePitch })}
                onTouchEnd={() => void patch({ voicePitch: settings.voicePitch })}
              />
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={settings.autoPlayAudio}
                onChange={(event) => void patch({ autoPlayAudio: event.target.checked })}
              />
              Auto-play TTS
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={settings.showSubtitles}
                onChange={(event) => void patch({ showSubtitles: event.target.checked })}
              />
              {locale === 'bn' ? 'সাবটাইটেল দেখাও' : 'Show subtitles'}
            </label>
          </div>
        </section>

        {/* Expressions */}
        <section className="card">
          <h2 className="mb-4 font-semibold">
            🎭 {t('settings.expressions')} {!flags.canAccessAllExpressions && `(${expressions.length}/12)`}
          </h2>
          <div className="flex flex-wrap gap-2">
            {expressions.map((expression) => (
              <button
                key={expression.id}
                type="button"
                onClick={() => void patch({ defaultExpression: normalizeExpression(expression.name) })}
                className={`rounded-full border px-3 py-1 text-xs ${
                  normalizeExpression(settings.defaultExpression) === normalizeExpression(expression.name)
                    ? 'border-brand bg-brand-soft text-brand-strong'
                    : 'border-line text-gray-300'
                }`}
              >
                {expression.displayName}
              </button>
            ))}
          </div>
        </section>

        {/* Animations */}
        <section className="card">
          <h2 className="mb-4 font-semibold">
            🕺 {t('settings.animations')} {!flags.canAccessAllAnimations && '🔒'}
          </h2>
          <div className="space-y-2">
            {animations.map((animation) => {
              const enabled = settings.enabledAnimations.includes(animation.name);
              const locked = !flags.canAccessAllAnimations && animation.name !== 'breathing';
              return (
                <label key={animation.id} className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={enabled}
                    disabled={locked}
                    onChange={(event) => {
                      const next = event.target.checked
                        ? [...settings.enabledAnimations, animation.name]
                        : settings.enabledAnimations.filter((name) => name !== animation.name);
                      void patch({ enabledAnimations: next.length ? next : ['breathing'] });
                    }}
                  />
                  {animation.displayName}
                  {locked && <span className="text-[10px] text-muted">🔒 subscriber</span>}
                </label>
              );
            })}
          </div>
          <div className="mt-4 space-y-3 border-t border-line pt-4 text-sm">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={settings.blinkEnabled}
                onChange={(event) => void patch({ blinkEnabled: event.target.checked })}
              />
              Blink
            </label>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={settings.thinkingPoseEnabled}
                onChange={(event) => void patch({ thinkingPoseEnabled: event.target.checked })}
              />
              {locale === 'bn' ? 'ভাবার ভঙ্গি' : 'Thinking pose'}
            </label>
          </div>
        </section>

        {/* Avatar model */}
        <section className="card">
          <h2 className="mb-4 font-semibold">🧑‍🚀 {t('settings.avatar')} {!flags.canSelectAvatarModel && '🔒'}</h2>
          <select
            className="input"
            value={settings.avatarModelId ?? ''}
            disabled={!flags.canSelectAvatarModel}
            onChange={(event) => {
              const value = event.target.value;
              void patch(value ? { avatarModelId: value } : { clearAvatarModel: true });
            }}
          >
            <option value="">Default</option>
            {avatars.map((avatar) => (
              <option key={avatar.id} value={avatar.id}>
                {avatar.name}
              </option>
            ))}
          </select>
        </section>

        {/* Custom AI key */}
        <section className="card">
          <h2 className="mb-4 font-semibold">
            🔑 {t('settings.customAi')} {!flags.canUseCustomApiKey && '🔒'}
          </h2>
          {flags.canUseCustomApiKey ? (
            <div className="space-y-3">
              <div>
                <label className="label">OpenRouter API Key</label>
                <input
                  className="input font-mono text-xs"
                  placeholder={customAi?.maskedApiKey ?? 'sk-or-v1-…'}
                  value={apiKey}
                  onChange={(event) => setApiKey(event.target.value)}
                />
              </div>
              <div>
                <label className="label">Preferred model</label>
                <input
                  className="input font-mono text-xs"
                  value={preferredModel}
                  onChange={(event) => setPreferredModel(event.target.value)}
                />
              </div>
              {customAi?.hasConfig && (
                <p className="text-xs text-muted">
                  {customAi.maskedApiKey} ·{' '}
                  {customAi.isVerified ? '✅ verified' : '⚠️ not verified'}
                </p>
              )}
              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  className="btn-primary"
                  onClick={async () => {
                    setError(null);
                    try {
                      setCustomAi(
                        await api.post<CustomAiConfigDto>('/custom-ai', {
                          apiKey,
                          preferredModel
                        })
                      );
                      setApiKey('');
                      flash(t('settings.saved'));
                    } catch (err) {
                      setError(err instanceof ApiError ? err.message : t('common.error'));
                    }
                  }}
                >
                  {t('settings.save')}
                </button>
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={async () => {
                    setError(null);
                    try {
                      const result = await api.post<{
                        isValid: boolean;
                        errorDetail?: string | null;
                      }>('/custom-ai/test');
                      flash(
                        result.isValid
                          ? locale === 'bn' ? 'কী সঠিক ✅' : 'Key is valid ✅'
                          : `❌ ${result.errorDetail ?? 'invalid key'}`
                      );
                    } catch (err) {
                      setError(err instanceof ApiError ? err.message : t('common.error'));
                    }
                  }}
                >
                  Test
                </button>
                <button
                  type="button"
                  className="btn-secondary text-danger"
                  onClick={async () => {
                    await api.delete('/custom-ai');
                    setCustomAi({ hasConfig: false, isVerified: false });
                    flash('Removed');
                  }}
                >
                  Delete
                </button>
              </div>
            </div>
          ) : (
            <p className="text-sm text-muted">
              {locale === 'bn'
                ? 'কাস্টম API কী শুধু সাবস্ক্রাইবারদের জন্য।'
                : 'Custom API keys are subscriber-only.'}
            </p>
          )}
        </section>
      </div>
    </div>
  );
}

export default function SettingsPage() {
  return (
    <RequireAuth>
      <SettingsInner />
    </RequireAuth>
  );
}
