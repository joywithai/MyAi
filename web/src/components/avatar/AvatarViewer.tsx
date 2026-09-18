'use client';

// 3D avatar canvas: loads a VRM model via @pixiv/three-vrm, applies expression
// recipes, lip-sync mouth value, breathing + blinking idle, and a thinking pose.
// If the model file cannot be loaded, a glowing "companion orb" is rendered so
// the experience still works without a .vrm asset.

import { useEffect, useRef, useState } from 'react';
import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { VRMLoaderPlugin, VRMUtils, type VRM } from '@pixiv/three-vrm';
import { getRecipe, normalizeExpression } from '@/lib/avatar/expressionMap';
import type { AvatarSceneConfig } from '@/lib/types';

export interface AvatarViewerProps {
  modelUrl?: string | null;
  /** Canonical expression name (NEUTRAL…SERIOUS) */
  expression: string;
  /** Mouth openness 0..1 driven by lip-sync */
  mouthOpen: number;
  /** While the AI is generating, show the thinking pose */
  thinking: boolean;
  blinkEnabled?: boolean;
  /** Admin-controlled scene (camera/model/lights). Undefined → defaults. */
  scene?: AvatarSceneConfig;
  className?: string;
}

interface ViewerState {
  sceneRoot: THREE.Scene;
  camera: THREE.PerspectiveCamera;
  renderer: THREE.WebGLRenderer;
  vrm: VRM | null;
  orb: THREE.Mesh | null;
  orbLight: THREE.PointLight | null;
  hemi: THREE.HemisphereLight;
  dir: THREE.DirectionalLight;
  disposed: boolean;
  blinkUntil: number;
  nextBlinkAt: number;
  currentPresets: Record<string, number>;
  targetPresets: Record<string, number>;
  mouth: number;
  mouthTarget: number;
  thinkingAmount: number;
  thinkingTarget: number;
}

const VRM_PRESET_ORDER = [
  'happy',
  'sad',
  'angry',
  'surprised',
  'relaxed',
  'aa',
  'ih',
  'ou',
  'ee',
  'oh',
  'blink'
];

/** Applies the admin scene config to camera, lights and (if loaded) the model. */
function applyScene(state: ViewerState, config?: AvatarSceneConfig) {
  if (!config) return;

  const { camera, hemi, dir } = state;

  camera.position.set(config.camera.x, config.camera.y, config.camera.z);
  camera.fov = config.camera.fov;
  camera.lookAt(config.camera.lookAtX, config.camera.lookAtY, config.camera.lookAtZ);
  camera.updateProjectionMatrix();

  hemi.intensity = config.lights.hemiIntensity;
  dir.position.set(config.lights.dirX, config.lights.dirY, config.lights.dirZ);
  dir.color.set(config.lights.dirColor);
  dir.intensity = config.lights.dirIntensity;

  if (state.vrm) {
    state.vrm.scene.position.set(config.model.x, config.model.y, config.model.z);
    state.vrm.scene.rotation.y = config.model.rotationY;
    state.vrm.scene.scale.setScalar(config.model.scale);
  }
}

export function AvatarViewer({
  modelUrl,
  expression,
  mouthOpen,
  thinking,
  blinkEnabled = true,
  scene,
  className
}: AvatarViewerProps) {
  const mountRef = useRef<HTMLDivElement>(null);
  const stateRef = useRef<ViewerState | null>(null);
  const [webglFailed, setWebglFailed] = useState(false);
  const propsRef = useRef({ expression, mouthOpen, thinking, blinkEnabled });
  const sceneRef = useRef<AvatarSceneConfig | undefined>(scene);

  propsRef.current = { expression, mouthOpen, thinking, blinkEnabled };
  sceneRef.current = scene;

  // ── Scene bootstrap ────────────────────────────────────────────────────────
  useEffect(() => {
    const mount = mountRef.current;
    if (!mount) return;

    const scene = new THREE.Scene();

    let renderer: THREE.WebGLRenderer;

    try {
      renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    } catch {
      // No WebGL available — the page still works, just without the 3D canvas.
      setWebglFailed(true);
      return;
    }

    const camera = new THREE.PerspectiveCamera(30, 1, 0.1, 20);
    camera.position.set(0, 1.35, 2.1);
    camera.lookAt(0, 1.05, 0);

    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.setSize(mount.clientWidth, mount.clientHeight);
    mount.appendChild(renderer.domElement);

    scene.add(new THREE.HemisphereLight(0xffffff, 0x33334d, 1.1));
    const hemi = scene.children[scene.children.length - 1] as THREE.HemisphereLight;

    const directional = new THREE.DirectionalLight(0x9b83ff, 1.6);
    directional.position.set(1.5, 2.5, 2);
    scene.add(directional);

    const state: ViewerState = {
      sceneRoot: scene,
      camera,
      renderer,
      vrm: null,
      orb: null,
      orbLight: null,
      hemi,
      dir: directional,
      disposed: false,
      blinkUntil: 0,
      nextBlinkAt: performance.now() + 2500,
      currentPresets: {},
      targetPresets: {},
      mouth: 0,
      mouthTarget: 0,
      thinkingAmount: 0,
      thinkingTarget: 0
    };
    stateRef.current = state;

    // Apply the admin-controlled scene (camera + lights) immediately.
    applyScene(state, sceneRef.current);

    // Placeholder companion orb (shown when no VRM is loaded)
    const orbGeometry = new THREE.SphereGeometry(0.32, 48, 48);
    const orbMaterial = new THREE.MeshStandardMaterial({
      color: 0x7c5cff,
      emissive: 0x7c5cff,
      emissiveIntensity: 0.7,
      roughness: 0.25
    });
    const orb = new THREE.Mesh(orbGeometry, orbMaterial);
    orb.position.set(0, 1.05, 0);
    scene.add(orb);
    state.orb = orb;

    const orbLight = new THREE.PointLight(0x7c5cff, 1.2, 6);
    orbLight.position.copy(orb.position);
    scene.add(orbLight);
    state.orbLight = orbLight;

    // ── Render loop ──────────────────────────────────────────────────────────
    const clock = new THREE.Clock();

    const animate = () => {
      if (state.disposed) return;
      requestAnimationFrame(animate);

      const delta = clock.getDelta();
      const elapsed = clock.elapsedTime;
      const { expression: exp, mouthOpen: mouth, thinking: think, blinkEnabled: blink } =
        propsRef.current;

      // Expression target recipe (lerped for smoothness)
      const recipe = getRecipe(normalizeExpression(exp));
      state.targetPresets = { ...recipe };

      const now = performance.now();
      if (blink && now >= state.nextBlinkAt && state.vrm) {
        state.blinkUntil = now + 140;
        state.nextBlinkAt = now + 2200 + Math.random() * 3600;
      }
      const blinking = blink && now < state.blinkUntil;
      state.targetPresets.blink = blinking ? 1 : 0;

      for (const preset of VRM_PRESET_ORDER) {
        const target = state.targetPresets[preset] ?? 0;
        const current = state.currentPresets[preset] ?? 0;
        state.currentPresets[preset] = current + (target - current) * Math.min(1, delta * 10);
      }

      // Mouth (lip-sync value from parent)
      state.mouthTarget = mouth;
      state.mouth += (state.mouthTarget - state.mouth) * Math.min(1, delta * 22);

      // Thinking pose blend
      state.thinkingTarget = think ? 1 : 0;
      state.thinkingAmount +=
        (state.thinkingTarget - state.thinkingAmount) * Math.min(1, delta * 5);

      const vrm = state.vrm;

      if (vrm) {
        // Apply expressions
        const manager = vrm.expressionManager;
        if (manager) {
          for (const [preset, value] of Object.entries(state.currentPresets)) {
            manager.setValue(preset, value);
          }
          if (manager.getExpression('aa')) {
            manager.setValue('aa', Math.max(state.currentPresets.aa ?? 0, state.mouth));
          }
        }

        // Breathing + thinking on normalized bones
        const humanoid = vrm.humanoid;
        if (humanoid) {
          const spine = humanoid.getNormalizedBoneNode('spine');
          if (spine) spine.rotation.z = Math.sin(elapsed * 1.4) * 0.025;

          const chest = humanoid.getNormalizedBoneNode('chest');
          if (chest) chest.rotation.x = Math.sin(elapsed * 1.4 + 0.6) * 0.012;

          const head = humanoid.getNormalizedBoneNode('head');
          if (head) {
            head.rotation.z =
              Math.sin(elapsed * 0.7) * 0.04 + state.thinkingAmount * 0.16;
            head.rotation.x = state.thinkingAmount * 0.12;
          }

          const leftUpperArm = humanoid.getNormalizedBoneNode('leftUpperArm');
          if (leftUpperArm) leftUpperArm.rotation.z = state.thinkingAmount * 0.9;

          const rightUpperArm = humanoid.getNormalizedBoneNode('rightUpperArm');
          if (rightUpperArm) rightUpperArm.rotation.z = -state.thinkingAmount * 1.3;

          const leftLowerArm = humanoid.getNormalizedBoneNode('leftLowerArm');
          if (leftLowerArm) leftLowerArm.rotation.y = state.thinkingAmount * 1.1;
        }

        if (vrm.lookAt && state.vrm) vrm.lookAt.target = camera;

        vrm.update(delta);
        state.orb!.visible = false;
        if (state.orbLight) state.orbLight.visible = false;
      } else {
        // Orb idle: gentle float + speech pulse
        const pulse = 1 + state.mouth * 0.28 + Math.sin(elapsed * 1.8) * 0.03;
        orb.scale.setScalar(pulse);
        orb.rotation.y = elapsed * 0.4;
        (orb.material as THREE.MeshStandardMaterial).emissiveIntensity =
          0.55 + state.mouth * 0.9 + Math.sin(elapsed * 1.8) * 0.08;
        if (state.orbLight) {
          state.orbLight.intensity = 1 + state.mouth * 2.2;
          state.orbLight.position.y = orb.position.y + Math.sin(elapsed * 1.2) * 0.05;
        }
        orb.position.y = 1.05 + Math.sin(elapsed * 1.2) * 0.04;
      }

      renderer.render(scene, camera);
    };

    animate();

    const onResize = () => {
      if (!mountRef.current) return;
      const { clientWidth, clientHeight } = mountRef.current;
      camera.aspect = clientWidth / clientHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(clientWidth, clientHeight);
    };
    const resizeObserver = new ResizeObserver(onResize);
    resizeObserver.observe(mount);

    return () => {
      state.disposed = true;
      resizeObserver.disconnect();
      VRMUtils.deepDispose(scene);
      renderer.dispose();
      if (renderer.domElement.parentElement === mount) {
        mount.removeChild(renderer.domElement);
      }
    };
  }, []);

  // ── Model loading ──────────────────────────────────────────────────────────
  useEffect(() => {
    const state = stateRef.current;
    if (!state || !modelUrl) return;

    let cancelled = false;

    const loader = new GLTFLoader();
    loader.register((parser) => new VRMLoaderPlugin(parser));

    loader.load(
      modelUrl,
      (gltf) => {
        if (cancelled || !stateRef.current) {
          VRMUtils.deepDispose(gltf.scene);
          return;
        }
        const vrm = gltf.userData.vrm as VRM | undefined;
        if (!vrm) return;

        VRMUtils.removeUnnecessaryVertices(gltf.scene);

        if (stateRef.current.vrm) {
          stateRef.current.sceneRoot.remove(stateRef.current.vrm.scene);
          VRMUtils.deepDispose(stateRef.current.vrm.scene);
        }

        vrm.scene.traverse((object) => {
          object.frustumCulled = false;
        });

        stateRef.current.sceneRoot.add(vrm.scene);
        stateRef.current.vrm = vrm;
        applyScene(stateRef.current, sceneRef.current);
        console.info('[MyAi] VRM avatar loaded from', modelUrl);
      },
      undefined,
      () => {
        // Load failure: keep the companion orb.
        console.warn(`[MyAi] Could not load VRM model from ${modelUrl}`);
      }
    );

    return () => {
      cancelled = true;
    };
  }, [modelUrl]);

  // ── Live scene updates (admin panel saves → everyone sees the change) ─────
  useEffect(() => {
    if (stateRef.current) applyScene(stateRef.current, scene);
  }, [scene]);

  if (webglFailed) {
    return (
      <div
        className={`flex items-center justify-center bg-gradient-to-b from-surface-raised to-surface ${className ?? ''}`}
        aria-label="Avatar (WebGL unavailable)"
      >
        <div className="text-center">
          <div className="text-6xl">🧑‍🚀</div>
          <p className="mt-2 text-xs text-muted">WebGL unavailable</p>
        </div>
      </div>
    );
  }

  return <div ref={mountRef} className={className} aria-label="3D avatar" />;
}
