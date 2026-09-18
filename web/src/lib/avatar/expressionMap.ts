// Maps the 12 canonical MyAi expressions (Domain.Enums.ExpressionType) onto
// VRM 1.0 standard blendshape presets. Unknown values normalise to NEUTRAL.

export interface ExpressionRecipe {
  /** VRM preset -> intensity (0..1) */
  presets: Record<string, number>;
}

export const CANONICAL_EXPRESSIONS = [
  'NEUTRAL',
  'HAPPY',
  'SAD',
  'SURPRISED',
  'ANGRY',
  'RELAXED',
  'EXCITED',
  'CONFUSED',
  'THOUGHTFUL',
  'CONCERNED',
  'FRIENDLY',
  'SERIOUS'
] as const;

export type CanonicalExpression = (typeof CANONICAL_EXPRESSIONS)[number];

const RECIPES: Record<CanonicalExpression, ExpressionRecipe> = {
  NEUTRAL: { presets: {} },
  HAPPY: { presets: { happy: 1.0 } },
  SAD: { presets: { sad: 1.0 } },
  SURPRISED: { presets: { surprised: 1.0, oh: 0.6 } },
  ANGRY: { presets: { angry: 1.0 } },
  RELAXED: { presets: { relaxed: 1.0 } },
  EXCITED: { presets: { happy: 0.9, surprised: 0.35, aa: 0.0 } },
  CONFUSED: { presets: { sad: 0.35, surprised: 0.45 } },
  THOUGHTFUL: { presets: { relaxed: 0.5, sad: 0.2 } },
  CONCERNED: { presets: { sad: 0.55, surprised: 0.15 } },
  FRIENDLY: { presets: { happy: 0.55 } },
  SERIOUS: { presets: { angry: 0.35, relaxed: 0.2 } }
};

export function normalizeExpression(raw: string | null | undefined): CanonicalExpression {
  if (!raw) return 'NEUTRAL';
  const value = raw.trim().toUpperCase();
  return (CANONICAL_EXPRESSIONS as readonly string[]).includes(value)
    ? (value as CanonicalExpression)
    : 'NEUTRAL';
}

export function getRecipe(expression: CanonicalExpression): Record<string, number> {
  return RECIPES[expression].presets;
}

/** Expression names available to public users (flag matrix §2.2). */
export const PUBLIC_EXPRESSIONS: CanonicalExpression[] = [
  'NEUTRAL',
  'HAPPY',
  'SAD',
  'SURPRISED'
];
