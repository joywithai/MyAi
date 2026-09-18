'use client';

import { useApp } from '@/lib/app-context';
import {
  CANONICAL_EXPRESSIONS,
  PUBLIC_EXPRESSIONS,
  normalizeExpression
} from '@/lib/avatar/expressionMap';

const ICONS: Record<string, string> = {
  NEUTRAL: '😐',
  HAPPY: '😊',
  SAD: '😢',
  SURPRISED: '😲',
  ANGRY: '😠',
  RELAXED: '😌',
  EXCITED: '🤩',
  CONFUSED: '😕',
  THOUGHTFUL: '🤔',
  CONCERNED: '🥺',
  FRIENDLY: '🙂',
  SERIOUS: '😑'
};

export function ExpressionChips({
  value,
  onChange,
  allAllowed
}: {
  value: string;
  onChange: (expression: string) => void;
  allAllowed: boolean;
}) {
  const { locale } = useApp();
  const allowed = allAllowed ? CANONICAL_EXPRESSIONS : PUBLIC_EXPRESSIONS;

  const labels =
    locale === 'bn'
      ? {
          NEUTRAL: 'স্বাভাবিক',
          HAPPY: 'খুশি',
          SAD: 'দুঃখিত',
          SURPRISED: 'অবাক',
          ANGRY: 'রাগান্বিত',
          RELAXED: 'শিথিল',
          EXCITED: 'উত্তেজিত',
          CONFUSED: 'বিভ্রান্ত',
          THOUGHTFUL: 'চিন্তাশীল',
          CONCERNED: 'উদ্বিগ্ন',
          FRIENDLY: 'বন্ধুত্বপূর্ণ',
          SERIOUS: 'গম্ভীর'
        }
      : {
          NEUTRAL: 'Neutral',
          HAPPY: 'Happy',
          SAD: 'Sad',
          SURPRISED: 'Surprised',
          ANGRY: 'Angry',
          RELAXED: 'Relaxed',
          EXCITED: 'Excited',
          CONFUSED: 'Confused',
          THOUGHTFUL: 'Thoughtful',
          CONCERNED: 'Concerned',
          FRIENDLY: 'Friendly',
          SERIOUS: 'Serious'
        };

  return (
    <div className="flex flex-wrap gap-2">
      {allowed.map((expression) => {
        const active = normalizeExpression(value) === expression;
        return (
          <button
            key={expression}
            type="button"
            onClick={() => onChange(expression)}
            className={`rounded-full border px-3 py-1 text-xs transition ${
              active
                ? 'border-brand bg-brand-soft text-brand-strong'
                : 'border-line text-gray-300 hover:border-brand'
            }`}
          >
            <span className="mr-1">{ICONS[expression]}</span>
            {labels[expression]}
          </button>
        );
      })}
      {!allAllowed && (
        <button
          type="button"
          disabled
          className="rounded-full border border-dashed border-line px-3 py-1 text-xs text-muted"
          title="Subscriber-only"
        >
          🔒 +8
        </button>
      )}
    </div>
  );
}
