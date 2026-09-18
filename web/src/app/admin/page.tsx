'use client';

// Admin panel (§8 admin surface): users, feature flags, expressions,
// animations, avatar models, system settings, audit logs.

import { useCallback, useEffect, useState } from 'react';
import { RequireAuth } from '@/components/layout/RequireAuth';
import { useApp } from '@/lib/app-context';
import { api, ApiError, qs } from '@/lib/api';
import type {
  AdminUserDto,
  AnimationDto,
  AuditLogDto,
  AvatarModelDto,
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
  | 'settings'
  | 'audit';

const TABS: Tab[] = ['users', 'flags', 'expressions', 'animations', 'avatars', 'settings', 'audit'];

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
    if (tab === 'settings' || tab === 'audit') void loadSystem();
  }, [tab, loadUsers, loadFlags, loadCatalog, loadSystem]);

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
            {t(`admin.${item === 'avatars' ? 'avatars' : item}`)}
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
