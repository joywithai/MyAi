import type { AvatarSceneConfig } from '../types';

/** Matches the backend AvatarSceneConfigDto defaults (seeded system setting). */
export const DEFAULT_SCENE: AvatarSceneConfig = {
  camera: { x: 0, y: 1.35, z: 2.1, lookAtX: 0, lookAtY: 1.05, lookAtZ: 0, fov: 30 },
  model: { x: 0, y: 0, z: 0, rotationY: 0, scale: 1 },
  lights: {
    hemiIntensity: 1.1,
    dirX: 1.5,
    dirY: 2.5,
    dirZ: 2,
    dirColor: '#9b83ff',
    dirIntensity: 1.6
  },
  canvasHeight: 430
};

/** Merges a (possibly partial) server config over the defaults. */
export function normalizeScene(partial?: Partial<AvatarSceneConfig> | null): AvatarSceneConfig {
  const source = partial ?? {};
  return {
    camera: { ...DEFAULT_SCENE.camera, ...(source.camera ?? {}) },
    model: { ...DEFAULT_SCENE.model, ...(source.model ?? {}) },
    lights: { ...DEFAULT_SCENE.lights, ...(source.lights ?? {}) },
    canvasHeight:
      typeof source.canvasHeight === 'number' && source.canvasHeight >= 240 && source.canvasHeight <= 720
        ? source.canvasHeight
        : DEFAULT_SCENE.canvasHeight
  };
}
