'use client';

// Drives the mouth openness value from an <audio> element + TTS word
// boundaries (offset/duration in ms, exactly as the backend returns them).

import { useEffect, useRef, useState } from 'react';
import type { WordBoundaryDto } from '@/lib/types';

export interface LipSyncState {
  /** 0..1 mouth openness for the current moment */
  mouthOpen: number;
  /** Currently spoken word (for subtitles) */
  currentWord: string | null;
  speaking: boolean;
  start: (audio: HTMLAudioElement, boundaries: WordBoundaryDto[]) => void;
  stop: () => void;
}

export function useLipSync(): LipSyncState {
  const [mouthOpen, setMouthOpen] = useState(0);
  const [currentWord, setCurrentWord] = useState<string | null>(null);
  const [speaking, setSpeaking] = useState(false);

  const rafRef = useRef<number | null>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const boundariesRef = useRef<WordBoundaryDto[]>([]);

  const stop = () => {
    if (rafRef.current !== null) {
      cancelAnimationFrame(rafRef.current);
      rafRef.current = null;
    }
    audioRef.current = null;
    boundariesRef.current = [];
    setSpeaking(false);
    setMouthOpen(0);
    setCurrentWord(null);
  };

  const start = (audio: HTMLAudioElement, boundaries: WordBoundaryDto[]) => {
    stop();
    audioRef.current = audio;
    boundariesRef.current = boundaries;
    setSpeaking(true);

    const tick = () => {
      const element = audioRef.current;
      if (!element || element.paused || element.ended) {
        stop();
        return;
      }

      const nowMs = element.currentTime * 1000;
      const active = boundariesRef.current.find(
        (boundary) =>
          nowMs >= boundary.offsetMs && nowMs < boundary.offsetMs + boundary.durationMs
      );

      if (active) {
        // Natural mouth flap: ~11 Hz modulated by position inside the word.
        const progress = (nowMs - active.offsetMs) / Math.max(1, active.durationMs);
        const envelope = Math.sin(progress * Math.PI);
        const flap = 0.55 + 0.45 * Math.abs(Math.sin(nowMs / 90));
        setMouthOpen(Math.min(1, envelope * flap));
        setCurrentWord(active.text);
      } else {
        setMouthOpen(0.05);
        setCurrentWord(null);
      }

      rafRef.current = requestAnimationFrame(tick);
    };

    rafRef.current = requestAnimationFrame(tick);
  };

  useEffect(() => stop, []);

  return { mouthOpen, currentWord, speaking, start, stop };
}
