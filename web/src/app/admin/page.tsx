'use client';

// Admin panel (§8 admin surface): users, feature flags, expressions,
// animations, avatar models, system settings, audit logs.

import { useCallback, useEffect, useState } from 'react';
import { RequireAuth } from '@/components/layout/RequireAuth';
import { useApp } from '@/lib/app-context';
import { api, ApiError, qs } from '@/lib/api';
import { AvatarViewer } from '@/components/avatar/AvatarViewer';
import { DEFAULT_SCENE, normalizeScene } from '@/lib/avatar/defaultScene';
import type {
  AdminUserDto,
  AnimationDto,
  AuditLogDto,
  AvatarModelDto,
  AvatarSceneConfig,
  ExpressionDto,
  FeatureFlagsDto,
  Paged,
  Role,
  SystemSettingDto
} from '@/lib/types';

type Tab =
  | 'users'
  | 'flags'
  | 'expressions'
  | 'animations'
  | 'avatars'
  | 'avatar-scene'
  | 'settings'
  | 'audit';

const TABS: Tab[] = ['users', 'flags', 'expressions', 'animations', 'avatars', 'avatar-scene', 'settings', 'audit'];

function AdminInner() {
  const { t, locale } = useApp();
  const [tab, setTab] = useState<Tab>('users');
  const [error, setError] = useState<string | null>(null);

  const notifyError = (err: unknown) =>
    setError(err instanceof ApiError ? err.message : t('common.error'));

  // ── Users ──────────────────────────────────────────────────────────────────
  const [users, setUsers] = useState<Paged<AdminUserDto> | null>(null);

  const loadUsers = useCallback(async () => {
    try {
      setUsers(await api.get<Paged<AdminUserDto>>(`/admin/users${qs({ page: 1, pageSize: 50 })}`));
    } catch (err) {
      notifyError(err);
    }
  }, [t]);

  // ── Flags ──────────────────────────────────────────────────────────────────
  const [flags, setFlags] = useState<FeatureFlagsDto[]>([]);

  const loadFlags = useCallback(async () => {
    try {
      setFlags(await api.get<FeatureFlagsDto[]>('/admin/feature-flags'));
    } catch (err) {
      notifyError(err);
    }
  }, [t]);

  // ── Catalog ────────────────────────────────────────────────────────────────
  const [expressions, setExpressions] = useState<ExpressionDto[]>([]);
  const [animations, setAnimations] = useState<AnimationDto[]>([]);
  const [avatars, setAvatars] = useState<AvatarModelDto[]>([]);
  const [newExpressionName, setNewExpressionName] = useState('');
  const [newAnimationName, setNewAnimationName] = useState('');
  const [newAvatarName, setNewAvatarName] = useState('');
  const [newAvatarUrl, setNewAvatarUrl] = useState('');

  const loadCatalog = useCallback(async () => {
    try {
      setExpressions(await api.get<ExpressionDto[]>('/admin/expressions'));
      setAnimations(await api.get<AnimationDto[]>('/admin/animations'));
      setAvatars(await api.get<AvatarModelDto[]>('/admin/avatar-models'));
    } catch (err) {
      notifyError(err);
    }
  }, [t]);

  // ── Avatar scene (global, DB-backed) ───────────────────────────────────────
  const [sceneConfig, setSceneConfig] = useState<AvatarSceneConfig>(DEFAULT_SCENE);
  const [sceneSaved, setSceneSaved] = useState(false);
  const [sceneBusy, setSceneBusy] = useState(false);

  const loadScene = useCallback(async () => {
    try {
      setSceneConfig(normalizeScene(await api.get<AvatarSceneConfig>('/avatars/scene')));
    } catch (err) {
      notifyError(err);
    }
  }, [t]);

  const saveScene = async () => {
    setSceneBusy(true);
    setError(null);
    try {
      const saved = await api.put<AvatarSceneConfig>('/admin/avatar-scene-config', sceneConfig);
      setSceneConfig(normalizeScene(saved));
      setSceneSaved(true);
      setTimeout(() => setSceneSaved(false), 2500);
    } catch (err) {
      notifyError(err);
    } finally {
      setSceneBusy(false);
    }
  };

  const sceneNumber = (path: string, value: number) => {
    setSceneConfig((current) => {
      const next = normalizeScene(current);
      const keys = path.split('.');
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      let target: any = next;
      for (let i = 0; i < keys.length - 1; i++) target = target[keys[i]];
      target[keys[keys.length - 1]] = value;
      return next;
    });
  };

  // ── System settings + audit ────────────────────────────────────────────────
  const [settings, setSettings] = useState<SystemSettingDto[]>([]);
  const [audit, setAudit] = useState<Paged<AuditLogDto> | null>(null);

  const loadSystem = useCallback(async () => {
    try {
      setSettings(await api.get<SystemSettingDto[]>('/admin/system-settings'));
      setAudit(await api.get<Paged<AuditLogDto>>(`/admin/audit-logs${qs({ page: 1, pageSize: 30 })}`));
    } catch (err) {
      notifyError(err);
    }
  }, [t]);

  useEffect(() => {
    setError(null);
    if (tab === 'users') void loadUsers();
    if (tab === 'flags') void loadFlags();
    if (tab === 'expressions' || tab === 'animations' || tab === 'avatars') void loadCatalog();
    if (tab === 'avatar-scene') void loadScene();
    if (tab === 'settings' || tab === 'audit') void loadSystem();
  }, [tab, loadUsers, loadFlags, loadCatalog, loadScene, loadSystem]);

  const updateSetting = async (key: string, value: string) => {
    try {
      await api.put(`/admin/system-settings/${encodeURIComponent(key)}`, { value });
      await loadSystem();
    } catch (err) {
      notifyError(err);
    }
  };

  const roleLabel = (role: Role) => t(`common.role.${role}`);

  return (
    <div className="space-y-6 py-2">
      <h1 className="text-2xl font-bold">{t('admin.title')}</h1>
      {error && <div className="rounded-lg bg-danger/10 px-4 py-3 text-sm text-danger">{error}</div>}

      <div className="flex flex-wrap gap-2">
        {TABS.map((item) => (
          <button
            key={item}
            type="button"
            onClick={() => setTab(item)}
            className={`rounded-lg border px-3 py-1.5 text-sm ${
              tab === item ? 'border-brand bg-brand-soft text-brand-strong' : 'border-line text-gray-300'
            }`}
          >
            {item === 'avatar-scene'
              ? locale === 'bn'
                ? 'অ্যাভাটার সিন'
                : 'Avatar scene'
              : t(`admin.${item}`)}
          </button>
        ))}
      </div>

      {/* Users */}
      {tab === 'users' && users && (
        <div className="card overflow-x-auto p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-line text-left text-xs uppercase text-muted">
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">{t('auth.displayName')}</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody>
              {users.items.map((user) => (
                <tr key={user.id} className="border-b border-line/50">
                  <td className="px-4 py-2 font-mono text-xs">{user.email}</td>
                  <td className="px-4 py-2">{user.displayName}</td>
                  <td className="px-4 py-2">
                    <select
                      className="input !w-auto !py-0.5 text-xs"
                      value={user.role}
                      onChange={async (event) => {
                        try {
                          await api.put(`/admin/users/${user.id}/role`, { role: event.target.value });
                          await loadUsers();
                        } catch (err) {
                          notifyError(err);
                        }
                      }}
                    >
                      {(['admin', 'subscriber', 'public_user'] as Role[]).map((role) => (
                        <option key={role} value={role}>{roleLabel(role)}</option>
                      ))}
                    </select>
                  </td>
                  <td className="px-4 py-2">
                    <span
                      className={user.status === 'Active' ? 'text-accent' : 'text-danger'}
                    >
                      {user.status}
                    </span>
                  </td>
                  <td className="px-4 py-2 text-right">
                    <button
                      type="button"
                      className="btn-secondary !px-2 !py-0.5 text-xs"
                      onClick={async () => {
                        const next = user.status === 'Active' ? 'Inactive' : 'Active';
                        try {
                          await api.put(`/admin/users/${user.id}/status`, { status: next });
                          await loadUsers();
                        } catch (err) {
                          notifyError(err);
                        }
                      }}
                    >
                      {user.status === 'Active' ? (locale === 'bn' ? 'নিষ্ক্রিয়' : 'Deactivate') : locale === 'bn' ? 'সক্রিয়' : 'Activate'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Feature flags */}
      {tab === 'flags' && (
        <div className="space-y-4">
          {flags.map((flag) => (
            <div key={flag.role} className="card">
              <h3 className="mb-3 font-semibold">{roleLabel(flag.role)}</h3>
              <div className="grid gap-2 sm:grid-cols-2">
                {(
                  [
                    ['canUseCustomApiKey', 'Custom API key'],
                    ['canAccessAllExpressions', 'All 12 expressions'],
                    ['canAccessAllAnimations', 'All animations'],
                    ['canSelectAvatarModel', 'Avatar selection'],
                    ['canCustomizeVoice', 'Voice customization'],
                    ['canAccessChatHistory', 'Chat history']
                  ] as const
                ).map(([key, label]) => (
                  <label key={key} className="flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={Boolean(flag[key])}
                      onChange={async (event) => {
                        try {
                          await api.put(`/admin/feature-flags/${flag.role}`, {
                            ...flag,
                            [key]: event.target.checked
                          });
                          await loadFlags();
                        } catch (err) {
                          notifyError(err);
                        }
                      }}
                    />
                    {label}
                  </label>
                ))}
                <label className="text-xs text-muted">
                  Max history
                  <input
                    className="input !py-1"
                    type="number"
                    defaultValue={flag.maxConversationHistory}
                    onBlur={async (event) => {
                      try {
                        await api.put(`/admin/feature-flags/${flag.role}`, {
                          ...flag,
                          maxConversationHistory: Number(event.target.value)
                        });
                      } catch (err) {
                        notifyError(err);
                      }
                    }}
                  />
                </label>
                <label className="text-xs text-muted">
                  Max messages/day (-1 ∞)
                  <input
                    className="input !py-1"
                    type="number"
                    defaultValue={flag.maxMessagesPerDay}
                    onBlur={async (event) => {
                      try {
                        await api.put(`/admin/feature-flags/${flag.role}`, {
                          ...flag,
                          maxMessagesPerDay: Number(event.target.value)
                        });
                      } catch (err) {
                        notifyError(err);
                      }
                    }}
                  />
                </label>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Expressions */}
      {tab === 'expressions' && (
        <div className="card">
          <div className="mb-4 flex gap-2">
            <input
              className="input"
              placeholder="NEW_EXPRESSION"
              value={newExpressionName}
              onChange={(event) => setNewExpressionName(event.target.value)}
            />
            <button
              type="button"
              className="btn-primary"
              onClick={async () => {
                if (!newExpressionName.trim()) return;
                try {
                  await api.post('/admin/expressions', {
                    name: newExpressionName.trim().toUpperCase(),
                    displayName: newExpressionName.trim(),
                    minRole: 'subscriber'
                  });
                  setNewExpressionName('');
                  await loadCatalog();
                } catch (err) {
                  notifyError(err);
                }
              }}
            >
              ＋
            </button>
          </div>
          <ul className="divide-y divide-line/50 text-sm">
            {expressions.map((expression) => (
              <li key={expression.id} className="flex items-center justify-between py-2">
                <span>
                  <span className="font-mono text-xs text-brand-strong">{expression.name}</span>{' '}
                  {expression.displayName}
                </span>
                <span className="text-xs text-muted">{expression.minRole}</span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Animations */}
      {tab === 'animations' && (
        <div className="card">
          <div className="mb-4 flex gap-2">
            <input
              className="input"
              placeholder="new-animation"
              value={newAnimationName}
              onChange={(event) => setNewAnimationName(event.target.value)}
            />
            <button
              type="button"
              className="btn-primary"
              onClick={async () => {
                if (!newAnimationName.trim()) return;
                try {
                  await api.post('/admin/animations', {
                    name: newAnimationName.trim().toLowerCase(),
                    displayName: newAnimationName.trim(),
                    minRole: 'subscriber'
                  });
                  setNewAnimationName('');
                  await loadCatalog();
                } catch (err) {
                  notifyError(err);
                }
              }}
            >
              ＋
            </button>
          </div>
          <ul className="divide-y divide-line/50 text-sm">
            {animations.map((animation) => (
              <li key={animation.id} className="flex items-center justify-between py-2">
                <span>
                  <span className="font-mono text-xs text-brand-strong">{animation.name}</span>{' '}
                  {animation.displayName}
                </span>
                <span className="text-xs text-muted">{animation.minRole}</span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Avatar models */}
      {tab === 'avatars' && (
        <div className="card">
          <div className="mb-4 flex flex-wrap gap-2">
            <input
              className="input !w-40"
              placeholder="Avatar name"
              value={newAvatarName}
              onChange={(event) => setNewAvatarName(event.target.value)}
            />
            <input
              className="input flex-1"
              placeholder="/models/avatar.vrm"
              value={newAvatarUrl}
              onChange={(event) => setNewAvatarUrl(event.target.value)}
            />
            <button
              type="button"
              className="btn-primary"
              onClick={async () => {
                if (!newAvatarName.trim() || !newAvatarUrl.trim()) return;
                try {
                  await api.post('/admin/avatar-models', {
                    name: newAvatarName.trim(),
                    fileUrl: newAvatarUrl.trim(),
                    minRole: 'public_user'
                  });
                  setNewAvatarName('');
                  setNewAvatarUrl('');
                  await loadCatalog();
                } catch (err) {
                  notifyError(err);
                }
              }}
            >
              ＋
            </button>
          </div>
          <ul className="divide-y divide-line/50 text-sm">
            {avatars.map((avatar) => (
              <li key={avatar.id} className="flex items-center justify-between py-2">
                <span>
                  {avatar.name}{' '}
                  {avatar.isDefault && (
                    <span className="rounded bg-brand-soft px-1 text-[10px] text-brand-strong">default</span>
                  )}
                  <div className="font-mono text-[11px] text-muted">{avatar.fileUrl}</div>
                </span>
                <span className="flex items-center gap-2 text-xs text-muted">
                  {avatar.minRole}
                  <button
                    type="button"
                    className="btn-secondary !px-2 !py-0.5 text-xs"
                    onClick={async () => {
                      try {
                        await api.delete(`/admin/avatar-models/${avatar.id}`);
                        await loadCatalog();
                      } catch (err) {
                        notifyError(err);
                      }
                    }}
                  >
                    ✕
                  </button>
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Avatar scene */}
      {tab === 'avatar-scene' && (
        <div className="grid gap-6 lg:grid-cols-[1fr_1fr]">
          {/* Live preview */}
          <div className="space-y-3">
            <div className="overflow-hidden rounded-2xl border border-line bg-gradient-to-b from-surface-raised to-surface">
              <AvatarViewer
                modelUrl="/models/AIAssistantAvatar.vrm"
                expression="FRIENDLY"
                mouthOpen={0}
                thinking={false}
                scene={sceneConfig}
                className="w-full"
              />
            </div>
            <p className="text-xs text-muted">
              {locale === 'bn'
                ? '⚠️ সেভ করলে সব ইউজারের কাছে এভাবেই দেখাবে। ইউজাররা এটা বদলাতে পারবে না।'
                : '⚠️ Saving applies this scene to every user. Users cannot change it.'}
            </p>
          </div>

          {/* Controls */}
          <div className="card space-y-4 text-sm">
            <h3 className="font-semibold">📷 {locale === 'bn' ? 'ক্যামেরা' : 'Camera'}</h3>
            <div className="grid grid-cols-3 gap-2">
              {(['x', 'y', 'z', 'lookAtX', 'lookAtY', 'lookAtZ'] as const).map((key) => (
                <label key={key} className="text-xs text-muted">
                  {key}
                  <input
                    className="input !py-1"
                    type="number"
                    step={0.05}
                    value={sceneConfig.camera[key]}
                    onChange={(event) => sceneNumber(`camera.${key}`, Number(event.target.value))}
                  />
                </label>
              ))}
              <label className="text-xs text-muted">
                FOV
                <input
                  className="input !py-1"
                  type="number"
                  step={1}
                  value={sceneConfig.camera.fov}
                  onChange={(event) => sceneNumber('camera.fov', Number(event.target.value))}
                />
              </label>
            </div>

            <h3 className="font-semibold">🧑‍🚀 {locale === 'bn' ? 'মডেল' : 'Model'}</h3>
            <div className="grid grid-cols-3 gap-2">
              {(['x', 'y', 'z', 'rotationY', 'scale'] as const).map((key) => (
                <label key={key} className="text-xs text-muted">
                  {key}
                  <input
                    className="input !py-1"
                    type="number"
                    step={key === 'scale' ? 0.05 : 0.05}
                    value={sceneConfig.model[key]}
                    onChange={(event) => sceneNumber(`model.${key}`, Number(event.target.value))}
                  />
                </label>
              ))}
            </div>

            <h3 className="font-semibold">💡 {locale === 'bn' ? 'আলো' : 'Lights'}</h3>
            <div className="grid grid-cols-3 gap-2">
              <label className="text-xs text-muted">
                hemi
                <input
                  className="input !py-1"
                  type="number"
                  step={0.1}
                  value={sceneConfig.lights.hemiIntensity}
                  onChange={(event) => sceneNumber('lights.hemiIntensity', Number(event.target.value))}
                />
              </label>
              {(['dirX', 'dirY', 'dirZ', 'dirIntensity'] as const).map((key) => (
                <label key={key} className="text-xs text-muted">
                  {key}
                  <input
                    className="input !py-1"
                    type="number"
                    step={0.1}
                    value={sceneConfig.lights[key]}
                    onChange={(event) => sceneNumber(`lights.${key}`, Number(event.target.value))}
                  />
                </label>
              ))}
              <label className="text-xs text-muted">
                dirColor
                <input
                  className="input !h-[34px] !py-0.5"
                  type="color"
                  value={sceneConfig.lights.dirColor}
                  onChange={(event) =>
                    setSceneConfig((current) => ({
                      ...current,
                      lights: { ...current.lights, dirColor: event.target.value }
                    }))
                  }
                />
              </label>
            </div>

            <label className="text-xs text-muted">
              {locale === 'bn' ? 'ক্যানভাস উচ্চতা (px)' : 'Canvas height (px)'}
              <input
                className="input !py-1 !w-32"
                type="number"
                step={10}
                value={sceneConfig.canvasHeight}
                onChange={(event) => sceneNumber('canvasHeight', Number(event.target.value))}
              />
            </label>

            <div className="flex items-center gap-3 pt-2">
              <button type="button" className="btn-primary" disabled={sceneBusy} onClick={() => void saveScene()}>
                {sceneBusy ? t('common.loading') : t('settings.save')}
              </button>
              <button type="button" className="btn-secondary" onClick={() => setSceneConfig(DEFAULT_SCENE)}>
                {locale === 'bn' ? 'ডিফল্টে ফেরান' : 'Reset to default'}
              </button>
              {sceneSaved && <span className="text-xs text-accent">{t('settings.saved')}</span>}
            </div>
          </div>
        </div>
      )}

      {/* System settings */}
      {tab === 'settings' && (
        <div className="card">
          <ul className="divide-y divide-line/50">
            {settings.map((setting) => (
              <li key={setting.key} className="flex flex-wrap items-center gap-2 py-2 text-sm">
                <span className="w-56 font-mono text-xs text-brand-strong">{setting.key}</span>
                <input
                  className="input !w-64 !py-1 font-mono text-xs"
                  defaultValue={setting.value}
                  onBlur={(event) => {
                    if (event.target.value !== setting.value) {
                      void updateSetting(setting.key, event.target.value);
                    }
                  }}
                />
                <span className="text-xs text-muted">{setting.description}</span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Audit logs */}
      {tab === 'audit' && audit && (
        <div className="card overflow-x-auto p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-line text-left text-xs uppercase text-muted">
                <th className="px-4 py-3">Time</th>
                <th className="px-4 py-3">Action</th>
                <th className="px-4 py-3">Details</th>
              </tr>
            </thead>
            <tbody>
              {audit.items.map((entry) => (
                <tr key={entry.id} className="border-b border-line/50">
                  <td className="px-4 py-2 text-xs text-muted">
                    {new Date(entry.createdAt).toLocaleString()}
                  </td>
                  <td className="px-4 py-2 font-mono text-xs text-brand-strong">{entry.action}</td>
                  <td className="px-4 py-2 text-xs text-muted">{entry.details}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default function AdminPage() {
  return (
    <RequireAuth minRole="admin">
      <AdminInner />
    </RequireAuth>
  );
}
