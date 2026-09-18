import { normalizeExpression, getRecipe, PUBLIC_EXPRESSIONS } from '../avatar/expressionMap';
import { DEFAULT_VRM_URL } from '../avatar/defaultAvatar';
import { translate } from '../i18n';
import { qs } from '../api';

describe('expressionMap', () => {
  it('keeps the 12 canonical expressions', () => {
    expect(PUBLIC_EXPRESSIONS).toEqual(['NEUTRAL', 'HAPPY', 'SAD', 'SURPRISED']);
  });

  it('normalizes case and unknown values to NEUTRAL', () => {
    expect(normalizeExpression('happy')).toBe('HAPPY');
    expect(normalizeExpression('WINK')).toBe('NEUTRAL');
    expect(normalizeExpression(null)).toBe('NEUTRAL');
  });

  it('maps HAPPY onto the VRM happy preset', () => {
    expect(getRecipe('HAPPY')).toEqual({ happy: 1.0 });
  });

  it('points at the bundled AIAssistantAvatar.vrm', () => {
    expect(DEFAULT_VRM_URL).toBe('/models/AIAssistantAvatar.vrm');
  });
});

describe('i18n', () => {
  it('translates known keys in Bangla and English', () => {
    expect(translate('bn', 'nav.chat')).toBe('চ্যাট');
    expect(translate('en', 'nav.chat')).toBe('Chat');
  });

  it('falls back to the key for unknown strings', () => {
    expect(translate('bn', 'missing.key')).toBe('missing.key');
  });
});

describe('qs', () => {
  it('skips undefined values', () => {
    expect(qs({ page: 1, pageSize: 20, role: undefined })).toBe('?page=1&pageSize=20');
  });

  it('returns empty string when nothing is set', () => {
    expect(qs({})).toBe('');
  });
});
