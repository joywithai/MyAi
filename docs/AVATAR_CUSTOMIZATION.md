# AIAssistantAvatar.vrm — কোড দিয়ে যা যা change করা যায়

> অ্যাভাটার: **"L"** (VRoid Studio 2.14.0, VRM 1.0) · উচ্চতা ~1.75m · T-pose · 55 humanoid bone (আঙুলসহ) · 14 preset expression · 57টা face morph · 15 material (মুখ, চোখ, চুল, শরীর, জামা, জুতা) · spring bone **নেই** (চুল static — চাইলে কোডে physics যোগ করা যাবে)

---

## ১. EXPRESSION (মুখের অভিব্যক্তি)

**ফাইল: `web/src/lib/avatar/expressionMap.ts`**

আমাদের ১২টা canonical expression → এই অ্যাভাটারের VRM preset এভাবে যায়:

| আমাদের Expression | VRM preset | অ্যাভাটারের Morph |
|---|---|---|
| HAPPY | `happy` | Fcl_ALL_Joy |
| SAD | `sad` | Fcl_ALL_Sorrow |
| ANGRY | `angry` | Fcl_ALL_Angry |
| SURPRISED | `surprised` | Fcl_ALL_Surprised |
| RELAXED | `relaxed` | Fcl_ALL_Fun |
| NEUTRAL | `neutral` | Fcl_ALL_Neutral |
| EXCITED | `happy 0.9 + surprised 0.35` | mix |
| CONFUSED | `sad 0.35 + surprised 0.45` | mix |
| THOUGHTFUL | `relaxed 0.5 + sad 0.2` | mix |
| CONCERNED | `sad 0.55 + surprised 0.15` | mix |
| FRIENDLY | `happy 0.55` | mix |
| SERIOUS | `angry 0.35 + relaxed 0.2` | mix |

**Change করতে:** `RECIPES` object-এর সংখ্যাগুলো বদলাও (0–1 intensity)। নতুন expression যোগ করতে `CANONICAL_EXPRESSIONS`-এ নাম + recipe যোগ করো (backend `ExpressionType` enum-এর সাথে মিলিয়ে)।

**Advanced — ৫৭টা face morph সরাসরি চালানো:** অ্যাভাটারে VRoid-এর সব morph আছে, three-vrm দিয়ে সরাসরি চালানো যায়:

```ts
// চোখের মণি লুকানো / হাইলাইট লুকানো / ভুরু আলাদা ইত্যাদি
import * as THREE from 'three';
const face = vrm.scene.getObjectByName('Face (merged)') as THREE.Mesh;
const mesh = face as unknown as { morphTargetDictionary?: Record<string, number>; morphTargetInfluences?: number[] };
const idx = mesh.morphTargetDictionary?.['Fcl_EYE_Iris_Hide'];
if (idx !== undefined) mesh.morphTargetInfluences![idx] = 1;
```

উল্লেখযোগ্য morphs: `Fcl_MTH_A/I/U/E/O` (মুখ), `Fcl_EYE_Close/_L/_R` (পলক), `Fcl_BRW_*` (ভুরু), `Fcl_HA_*` (আগের চুল মুভ), `Fcl_EYE_Iris_Hide`, `Fcl_EYE_Highlight_Hide`।

---

## ২. LIP-SYNC (কথার সাথে মুখ নড়া)

**ফাইল: `web/src/hooks/useLipSync.ts`**

| কী change হয় | কোথায় | এখনকার মান |
|---|---|---|
| মুখ খোলার তীব্রতা | `flap = 0.55 + 0.45 * ...` | 0.05–1.0 |
| মুখ নড়ার গতি | `Math.sin(nowMs / 90)` | ~11 Hz |
| word boundary timing | backend থেকে আসে | 320ms/শব্দ (demo) |

Vowel morphs: lip-sync মূল `aa` preset চালায়; চাইলে `ih/ou/ee/oh`-ও রোটেট করতে পারো (AvatarViewer-এ `state.mouth` ব্যবহারের জায়গায়)।

---

## ৩. ANIMATION / POSE (হাড়ের নড়াচড়া)

**ফাইল: `web/src/components/avatar/AvatarViewer.tsx` — `animate()` লুপ**

| Animation | Bone | এখনকার মান |
|---|---|---|
| Breathing (শ্বাস) | `spine.rotation.z` | sin(t·1.4)·0.025 rad |
| Chest | `chest.rotation.x` | sin(t·1.4+0.6)·0.012 |
| Idle head sway | `head.rotation.z` | sin(t·0.7)·0.04 |
| Thinking pose | `head.rotation.z/x`, দুই বাহু | tilt 0.16, arm 0.9/1.3 rad |
| Blink | `blink` preset | প্রতি 2.2–5.8s, 140ms |

**যেকোনো bone নাড়ানো যায়** (hips, head, আঙুল পর্যন্ত ৫৫টা):

```ts
const head = vrm.humanoid.getNormalizedBoneNode('head');
head.rotation.y = 0.3;         // মাথা ডানে ঘোরাও
const hand = vrm.humanoid.getNormalizedBoneNode('leftHand');
hand.rotation.x = -0.5;        // হাত নাড়াও (wave বানাতে)
```

**Wave অ্যানিমেশনের উদাহরণ:** `rightUpperArm.rotation.z = -2.2` + `rightLowerArm.rotation.y = sin(t*6)*0.5`।

চুল/জামার physics: এই মডেলে spring bone নেই; `vrm.springBoneManager` null হবে। নিজে চাইলে secondary motion কোড করতে হবে।

---

## ৪. POSITION MAP — কে কোথায় দাঁড়িয়ে (মিটার, Y-up)

> **⚡ নতুন:** নিচের সব camera/model/lights/ক্যানভাস মান এখন **Admin panel → "অ্যাভাটার সিন" ট্যাব** থেকে UI দিয়েই বদলানো যায় — সেভ করলে `SystemSetting("avatar_scene_config")` (DB)-তে জমা হয় এবং **সব ইউজারের কাছে সেভাবেই** দেখায়। ইউজাররা এটা বদলাতে পারে না (`PUT /api/v1/admin/avatar-scene-config` → admin-only 403)। API: `GET /api/v1/avatars/scene` (সবাই) / `PUT /api/v1/admin/avatar-scene-config` (শুধু admin)। কোড-লেভেল ডিফল্ট: `web/src/lib/avatar/defaultScene.ts` ⇄ `Application/Features/Avatar/Dtos/AvatarSceneConfigDto.cs`।


### অ্যাভাটারের ভেতরের জয়েন্টগুলোর আসল position (T-pose rest)

| Joint | Position (x, y, z) |
|---|---|
| hips | (0.000, **1.073**, 0.004) |
| spine | (0.000, 1.138, 0.011) |
| chest | (0.000, 1.274, −0.006) |
| upperChest | (0.000, 1.406, −0.008) |
| neck | (0.000, 1.573, 0.001) |
| **head** | (0.000, **1.666**, 0.002) |
| leftEye / rightEye | (±0.017, **1.741**, 0.026) |
| shoulders | (±0.027, 1.539, 0.000) |
| hands (T-pose) | (±**0.669**, 1.539, 0.000) |
| fingers-এর ডগা | x ±0.82 পর্যন্ত |
| upperLeg / knee / foot | y 1.025 / 0.619 / 0.121 |

মানে অ্যাভাটার **মাটি (y=0) থেকে দাঁড়িয়ে**, মুখ **+Z দিকে**, মোট উচ্চতা ~1.75m।

### Scene-এ ক্যামেরা / আলো / মডেল (`AvatarViewer.tsx`)

| জিনিস | Position | কী change করলে কী হয় |
|---|---|---|
| **Camera** | `(0, 1.35, 2.1)`, lookAt `(0, 1.05, 0)`, FOV 30° | কাছে আনতে z কমাও (2.1→1.6), উপরে-নিচে দেখতে y/lookAt বদলাও, বড় দেখাতে FOV কমাও |
| **Model** | origin `(0,0,0)` | `vrm.scene.position.set(x,y,z)`; ঘোরাতে `rotation.y`; ছোট/বড় `scale.setScalar()` |
| **Hemisphere light** | scene-wide, intensity 1.1 | পরিবেশের আলো |
| **Directional light** | `(1.5, 2.5, 2)`, রঙ `#9b83ff`, intensity 1.6 | মূল আলো — রঙ/তীব্রতা/দিক |
| **Orb fallback** | `(0, 1.05, 0)` (মডেল লোড fail হলে) | radius 0.32, রঙ `#7c5cff` |

### UI-তে ক্যানভাস কোথায়

- Chat page: বাম কলাম, `h-[380px] sm:h-[430px]`, রাউন্ডেড dark gradient panel — `web/src/app/chat/page.tsx`
- পুরো পেজ max-width 1000px — `web/tailwind.config.ts` (`maxWidth.page`)

---

## ৫. LOOK-AT (চোখ কার দিকে তাকায়)

`AvatarViewer.tsx`: `vrm.lookAt.target = camera` — অ্যাভাটার **সবসময় ক্যামেরার দিকে তাকায়** (মানে তোমার দিকে)। অন্য জিনিসের দিকে তাকাতে:

```ts
const target = new THREE.Object3D();
target.position.set(1, 1.6, 0);   // ডান দিকে তাকাও
scene.add(target);
vrm.lookAt.target = target;
```

---

## ৬. MATERIAL / COLOR (পোশাক-চুল-চোখের রঙ)

১৫টা material: Face, FaceMouth, EyeIris, EyeHighlight, EyeWhite, FaceBrow, FaceEyelash, FaceEyeline, Body(SKIN), Tops(CLOTH), Shoes(CLOTH), HairBack(HAIR), Onepiece(CLOTH ×2), Hair(HAIR)।

```ts
vrm.scene.traverse((obj) => {
  const mesh = obj as THREE.Mesh;
  if (!mesh.isMesh) return;
  for (const mat of Array.isArray(mesh.material) ? mesh.material : [mesh.material]) {
    const m = mat as THREE.MeshStandardMaterial;
    if (m?.name?.includes('HairBack')) m.color.set('#ff69b4');  // চুল গোলাপি!
    if (m?.name?.includes('Onepiece')) m.color.set('#222');     // জামা কালো
  }
});
```

⚠️ VRoid material-গুলোতে texture-আধারিত shading থাকে, তাই রঙ tint হিসেবে বসে — সম্পূর্ণ বদলাতে texture swap লাগে।

---

## ৭. MODEL SWAP (অন্য অ্যাভাটার)

| Layer | ফাইল |
|---|---|
| কোন ফাইল লোড হয় | `web/src/lib/avatar/defaultAvatar.ts` → `DEFAULT_VRM_URL` |
| ইউজার-নির্বাচিত মডেল | `PUT /api/v1/avatars/select` (flag: `can_select_avatar_model`) |
| Catalog | DB `avatar_models` টেবিল (admin CRUD) + `DatabaseSeeder.SeedAvatarModelsAsync` |
| ফাইল ফোল্ডার | `web/public/models/` — নতুন `.vrm` এখানে রেখে নতুন মডেল যোগ করো |

---

## ৮. ভয়েস (ঠোঁট নড়ানোর অডিও সোর্স)

| কী | ফাইল |
|---|---|
| ভয়েস নির্বাচন (Nabanita/Jenny…) | `Domain/ValueObjects/VoiceSettings` + `Infrastructure/TTS/Shared/VoiceSelector` |
| Speed/Pitch | user settings → Edge TTS SSML |
| Word boundary timing | `EdgeTtsWebSocketClient` (ticks→ms) |
| Demo audio | `demo/mock-server.mjs` → `makeDemoWav()` (untracked) |

---

## ৯. QUICK RECIPE — সবচেয়ে কমন পরিবর্তন

```ts
// ১. অ্যাভাটারকে বড়/ছোট করা
vrm.scene.scale.setScalar(0.9);

// ২. একটু বাঁ দিকে সরানো + ঘোরানো
vrm.scene.position.x = 0.3;
vrm.scene.rotation.y = 0.4;      // radian (~23°)

// ৩. ক্যামেরা কাছে আনা (close-up)
camera.position.set(0, 1.5, 1.4);
camera.lookAt(0, 1.45, 0);       // মুখে ফোকাস

// ৪. স্মাইল ফোর্স করা (AI যা-ই বলুক)
vrm.expressionManager?.setValue('happy', 1);

// ৫. এক চোখ কুড়ানো (wink)
vrm.expressionManager?.setValue('blinkLeft', 1);

// ৬. মাথা নিচু করা (লজ্জা)
vrm.humanoid.getNormalizedBoneNode('head')!.rotation.x = 0.3;
```

সব animation change করার পর hot reload-ই যথেষ্ট (`npm run dev`), নতুন `.vrm` ফাইল হলে `web/public/models/`-এ রেখে `DEFAULT_VRM_URL` আপডেট করো।
