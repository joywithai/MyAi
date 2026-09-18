'use client';

// Main chat experience: avatar + expression picker + voice controls on the
// left, conversation on the right. Dark-only, max 1000px.

import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { AvatarViewer } from '@/components/avatar/AvatarViewer';
import { ExpressionChips } from '@/components/chat/ExpressionChips';
import { MessageBubble } from '@/components/chat/MessageBubble';
import { Plus, Send, Square } from 'lucide-react';
import { useApp } from '@/lib/app-context';
import { useChat } from '@/hooks/useChat';
import { useLipSync } from '@/hooks/useLipSync';
import { api } from '@/lib/api';
import { normalizeExpression } from '@/lib/avatar/expressionMap';
import { DEFAULT_VRM_URL } from '@/lib/avatar/defaultAvatar';
import { normalizeScene } from '@/lib/avatar/defaultScene';
import type {
  AnimationDto,
  AvatarSceneConfig,
  ExpressionDto,
  FeatureFlagsDto,
  Language,
  UserSettingsDto
} from '@/lib/types';

export default function ChatPage() {
  const { user, t } = useApp();
  const lipSync = useLipSync();
  const [language, setLanguage] = useState<Language>('bn');
  const chat = useChat(language);
  const [input, setInput] = useState('');
  const [expression, setExpression] = useState('FRIENDLY');
  const [settings, setSettings] = useState<UserSettingsDto | null>(null);
  const [flags, setFlags] = useState<FeatureFlagsDto | null>(null);
  const [animations, setAnimations] = useState<AnimationDto[]>([]);
  const [modelUrl, setModelUrl] = useState<string | null>(DEFAULT_VRM_URL);
  const [sceneConfig, setSceneConfig] = useState<AvatarSceneConfig | undefined>(undefined);
  const [subtitles, setSubtitles] = useState<string | null>(null);
  const bottomRef = useRef<HTMLDivElement>(null);

  const playableAudio = useMemo(() => {
    const last = [...chat.messages].reverse().find((message) => message.role === 'assistant');
    if (!last) return null;
    if (last.audioBase64) {
      return `data:${last.audioContentType ?? 'audio/mpeg'};base64,${last.audioBase64}`;
    }
    if (last.audioUrl) return last.audioUrl;
    return null;
  }, [chat.messages]);

  // Load settings, flags and catalog on entry
  useEffect(() => {
    void (async () => {
      try {
        const userSettings = await api.get<UserSettingsDto>('/settings');
        setSettings(userSettings);
        setLanguage(userSettings.preferredLanguage);
        setExpression(userSettings.defaultExpression ?? 'FRIENDLY');
      } catch {
        /* defaults */
      }
      try {
        setFlags(await api.get<FeatureFlagsDto>('/settings/feature-flags'));
      } catch {
        /* ignore */
      }
      try {
        setAnimations(await api.get<AnimationDto[]>('/animations'));
      } catch {
        /* ignore */
      }
      try {
        const models = await api.get<Array<{ fileUrl: string }>>('/avatars/models');
        const selected = models.find((model) => model.fileUrl);
        if (selected) setModelUrl(selected.fileUrl);
      } catch {
        /* orb fallback */
      }
    })();
  }, []);

  // Load conversation history only when the flag allows it
  useEffect(() => {
    void chat.loadConversations(flags?.canAccessChatHistory ?? false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [flags?.canAccessChatHistory]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [chat.messages, chat.typing]);

  const handleSend = useCallback(async () => {
    const text = input.trim();
    if (!text) return;
    setInput('');
    setSubtitles(null);

    const response = await chat.send(text);
    if (!response) return;

    setExpression(normalizeExpression(response.message.expression));

    // Auto-play TTS when the backend produced audio
    if (settings?.autoPlayAudio !== false && (response.message.audioBase64 || response.message.audioUrl)) {
      const audio = new Audio(
        response.message.audioBase64
          ? `data:${response.message.audioContentType ?? 'audio/mpeg'};base64,${response.message.audioBase64}`
          : (response.message.audioUrl as string)
      );
      audio.play().catch(() => undefined);
      lipSync.start(audio, response.message.wordBoundaries ?? []);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [input, chat, settings, lipSync]);

  const activeWord = lipSync.currentWord;
  useEffect(() => {
    if (settings?.showSubtitles !== false && activeWord) setSubtitles(activeWord);
  }, [activeWord, settings?.showSubtitles]);

  const canSend = input.trim().length > 0 && !chat.typing && !chat.dailyLimitReached;

  return (
    <div className="grid gap-6 py-2 lg:grid-cols-[1.05fr_1fr]">
      {/* ── Avatar side ── */}
      <section className="flex flex-col gap-4">
        <div
          className="relative overflow-hidden rounded-2xl border border-line bg-gradient-to-b from-surface-raised to-surface"
          style={{ height: sceneConfig?.canvasHeight ?? 430 }}
        >
          <AvatarViewer
            modelUrl={modelUrl}
            expression={expression}
            mouthOpen={lipSync.mouthOpen}
            thinking={chat.typing}
            blinkEnabled={settings?.blinkEnabled ?? true}
            scene={sceneConfig}
            className="h-full w-full"
          />
          {settings?.showSubtitles !== false && subtitles && (
            <div className="pointer-events-none absolute bottom-3 left-1/2 -translate-x-1/2 rounded-lg bg-black/70 px-3 py-1 text-sm text-white">
              {subtitles}
            </div>
          )}
          {chat.typing && (
            <div className="absolute left-3 top-3 rounded-lg bg-brand-soft px-2 py-1 text-xs text-brand-strong">
              {t('chat.thinking')}
            </div>
          )}
          {lipSync.speaking && (
            <div className="absolute right-3 top-3 flex items-center gap-1 rounded-lg bg-black/60 px-2 py-1 text-xs text-accent">
              <span className="inline-block h-2 w-2 animate-pulse rounded-full bg-accent" />
              TTS
            </div>
          )}
        </div>

        <ExpressionChips
          value={expression}
          onChange={setExpression}
          allAllowed={flags?.canAccessAllExpressions ?? false}
        />

        {/* Voice / language quick controls */}
        <div className="card flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2">
            <span className="text-xs text-muted">{t('settings.language')}</span>
            <button
              type="button"
              onClick={() => setLanguage(language === 'bn' ? 'en' : 'bn')}
              className="btn-secondary !px-3 !py-1 text-xs"
            >
              {language === 'bn' ? 'বাংলা' : 'English'}
            </button>
          </div>

          {settings && (flags?.canCustomizeVoice ?? false) && (
            <>
              <label className="flex items-center gap-2 text-xs text-muted">
                Speed {settings.voiceSpeed.toFixed(2)}×
                <input
                  type="range"
                  min={0.5}
                  max={2}
                  step={0.05}
                  value={settings.voiceSpeed}
                  onChange={(event) =>
                    setSettings({ ...settings, voiceSpeed: Number(event.target.value) })
                  }
                  onMouseUp={() => void api.put('/settings', { voiceSpeed: settings.voiceSpeed })}
                  onTouchEnd={() => void api.put('/settings', { voiceSpeed: settings.voiceSpeed })}
                />
              </label>
              <label className="flex items-center gap-2 text-xs text-muted">
                Pitch {settings.voicePitch}
                <input
                  type="range"
                  min={-50}
                  max={50}
                  step={5}
                  value={settings.voicePitch}
                  onChange={(event) =>
                    setSettings({ ...settings, voicePitch: Number(event.target.value) })
                  }
                  onMouseUp={() => void api.put('/settings', { voicePitch: settings.voicePitch })}
                  onTouchEnd={() => void api.put('/settings', { voicePitch: settings.voicePitch })}
                />
              </label>
            </>
          )}

          {flags?.canAccessAllAnimations === false && animations.length > 0 && (
            <span className="text-xs text-muted">
              🎬 {t('settings.animations')}: breathing
            </span>
          )}
        </div>
      </section>

      {/* ── Conversation side ── */}
      <section className="flex flex-col rounded-2xl border border-line bg-surface-raised">
        <div className="flex items-center justify-between border-b border-line px-4 py-3">
          <h2 className="text-sm font-semibold">{t('nav.chat')}</h2>
          {flags?.canAccessChatHistory ? (
            <div className="flex items-center gap-2">
              <select
                className="input !w-auto !py-1 text-xs"
                value={chat.conversationId ?? ''}
                onChange={(event) => {
                  const id = event.target.value;
                  if (id) void chat.openConversation(id);
                }}
              >
                <option value="">— {t('common.loading')} —</option>
                {chat.conversations.map((conversation) => (
                  <option key={conversation.id} value={conversation.id}>
                    {conversation.title || conversation.id.slice(0, 8)}
                  </option>
                ))}
              </select>
              <button type="button" onClick={chat.newConversation} className="btn-secondary !py-1 text-xs" aria-label="New conversation">
                <Plus className="h-3.5 w-3.5" />
              </button>
            </div>
          ) : (
            <span className="text-xs text-muted">🔒 history locked</span>
          )}
        </div>

        <div className="flex-1 space-y-3 overflow-y-auto px-4 py-4" style={{ maxHeight: 480 }}>
          {chat.messages.length === 0 && !chat.typing && (
            <div className="py-16 text-center text-sm text-muted">
              {language === 'bn'
                ? 'আপনার AI সঙ্গীকে কিছু বলুন…'
                : 'Say something to your AI companion…'}
            </div>
          )}
          {chat.messages.map((message) => (
            <MessageBubble key={message.id} message={message} />
          ))}
          {chat.typing && (
            <div className="flex items-center gap-2 text-xs text-muted">
              <span className="h-2 w-2 animate-bounce rounded-full bg-brand" />
              <span className="h-2 w-2 animate-bounce rounded-full bg-brand [animation-delay:120ms]" />
              <span className="h-2 w-2 animate-bounce rounded-full bg-brand [animation-delay:240ms]" />
            </div>
          )}
          <div ref={bottomRef} />
        </div>

        {chat.dailyLimitReached && (
          <div className="mx-4 mb-2 rounded-lg bg-danger/10 px-3 py-2 text-xs text-danger">
            {t('chat.dailyLimit')} — /subscription
          </div>
        )}

        <form
          className="flex items-end gap-2 border-t border-line p-3"
          onSubmit={(event) => {
            event.preventDefault();
            void handleSend();
          }}
        >
          <textarea
            className="input min-h-[44px] max-h-32 flex-1 resize-none"
            placeholder={t('chat.placeholder')}
            value={input}
            maxLength={500}
            rows={1}
            onChange={(event) => setInput(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === 'Enter' && !event.shiftKey) {
                event.preventDefault();
                void handleSend();
              }
            }}
          />
          {chat.typing ? (
            <button type="button" className="btn-secondary" onClick={chat.stop}>
              <Square className="h-4 w-4" /> {t('chat.stop')}
            </button>
          ) : (
            <button type="submit" className="btn-primary" disabled={!canSend}>
              <Send className="h-4 w-4" /> {t('chat.send')}
            </button>
          )}
        </form>
        <div className="px-4 pb-2 text-right text-[10px] text-muted">
          {user?.email} · {input.length}/500
        </div>
      </section>
    </div>
  );
}
