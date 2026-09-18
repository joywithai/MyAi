'use client';

import type { ChatMessage } from '@/hooks/useChat';

export function MessageBubble({ message }: { message: ChatMessage }) {
  const isAssistant = message.role === 'assistant';

  return (
    <div className={`flex ${isAssistant ? 'justify-start' : 'justify-end'}`}>
      <div
        className={`max-w-[80%] rounded-2xl px-4 py-2.5 text-sm leading-relaxed ${
          isAssistant
            ? message.failed
              ? 'border border-danger/40 bg-danger/10 text-danger'
              : 'rounded-bl-md bg-surface-overlay text-gray-100'
            : 'rounded-br-md bg-brand text-white'
        }`}
      >
        {isAssistant && message.expression && !message.failed && (
          <div className="mb-1 text-[10px] uppercase tracking-wider text-brand-strong">
            {message.expression}
          </div>
        )}
        <p className="whitespace-pre-wrap">{message.content}</p>
      </div>
    </div>
  );
}
