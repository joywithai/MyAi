'use client';

// Chat state machine: send → typing → play TTS with lip-sync data.

import { useCallback, useEffect, useRef, useState } from 'react';
import { api, ApiError } from '@/lib/api';
import type { ChatResponse, ConversationDto, Language, MessageDto, Paged } from '@/lib/types';

export interface ChatMessage extends MessageDto {
  pending?: boolean;
  failed?: boolean;
}

export function useChat(language: Language) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [conversationId, setConversationId] = useState<string | null>(null);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [typing, setTyping] = useState(false);
  const [dailyLimitReached, setDailyLimitReached] = useState(false);
  const abortRef = useRef<AbortController | null>(null);

  const loadConversations = useCallback(async (enabled: boolean) => {
    if (!enabled) return;
    try {
      const page = await api.get<Paged<ConversationDto>>('/conversations?pageSize=20');
      setConversations(page.items);
    } catch {
      /* history optional */
    }
  }, []);

  const openConversation = useCallback(async (id: string) => {
    const history = await api.get<Paged<MessageDto>>(
      `/conversations/${id}/messages?pageSize=50`
    );
    setConversationId(id);
    setMessages(history.items);
  }, []);

  const newConversation = useCallback(() => {
    setConversationId(null);
    setMessages([]);
  }, []);

  const stop = useCallback(() => {
    abortRef.current?.abort();
    abortRef.current = null;
    setTyping(false);
    void api.post('/chat/stop').catch(() => undefined);
  }, []);

  const send = useCallback(
    async (text: string): Promise<ChatResponse | null> => {
      const trimmed = text.trim();
      if (!trimmed || typing) return null;

      const userMessage: ChatMessage = {
        id: `local-${Date.now()}`,
        conversationId: conversationId ?? '',
        role: 'user',
        content: trimmed,
        language,
        expression: null,
        createdAt: new Date().toISOString()
      };
      setMessages((previous) => [...previous, userMessage]);
      setTyping(true);

      const controller = new AbortController();
      abortRef.current = controller;

      try {
        const response = await api.post<ChatResponse>(
          '/chat/ask',
          { message: trimmed, language, conversationId },
          controller.signal
        );

        setConversationId(response.conversationId);
        setMessages((previous) => [...previous, response.message]);
        setDailyLimitReached(false);
        return response;
      } catch (error) {
        if (error instanceof ApiError && error.status === 429) {
          setDailyLimitReached(true);
        } else if (!(error instanceof DOMException && error.name === 'AbortError')) {
          setMessages((previous) => [
            ...previous,
            {
              ...userMessage,
              id: `error-${Date.now()}`,
              role: 'assistant',
              content: 'দুঃখিত, একটি সমস্যা হয়েছে। আবার চেষ্টা করুন। / Sorry, something went wrong.',
              failed: true
            }
          ]);
        }
        return null;
      } finally {
        setTyping(false);
        abortRef.current = null;
      }
    },
    [conversationId, language, typing]
  );

  useEffect(() => () => abortRef.current?.abort(), []);

  return {
    messages,
    conversationId,
    conversations,
    typing,
    dailyLimitReached,
    send,
    stop,
    openConversation,
    newConversation,
    loadConversations
  };
}
