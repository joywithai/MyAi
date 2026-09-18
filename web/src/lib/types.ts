// Shared frontend types mirroring the API contracts (§8).

export type Role = 'admin' | 'subscriber' | 'public_user';
export type Language = 'bn' | 'en';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  role: Role;
  status: string;
  emailVerified: boolean;
  avatarUrl?: string | null;
  createdAt: string;
  subscription?: SubscriptionDto | null;
}

export interface ConversationDto {
  id: string;
  title: string;
  userId: string;
  messageCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface MessageDto {
  id: string;
  conversationId: string;
  role: string; // user | assistant | system
  content: string;
  language: Language;
  expression: string | null;
  audioUrl?: string | null;
  audioBase64?: string | null;
  audioContentType?: string | null;
  wordBoundaries?: WordBoundaryDto[];
  createdAt: string;
}

export interface WordBoundaryDto {
  text: string;
  offsetMs: number;
  durationMs: number;
}

export interface ChatResponse {
  conversationId: string;
  message: MessageDto; // assistant reply
  userMessage: MessageDto;
  remainingMessages: number;
}

export interface ExpressionSegmentDto {
  expression: string;
  text: string;
}

export interface UserSettingsDto {
  preferredLanguage: Language;
  voiceName: string;
  voiceSpeed: number;
  voicePitch: number;
  defaultExpression: string;
  themePreference: string;
  showSubtitles: boolean;
  autoPlayAudio: boolean;
  enabledAnimations: string[];
  blinkEnabled: boolean;
  blinkFrequency: number;
  thinkingPoseEnabled: boolean;
  avatarModelId?: string | null;
}

export interface UpdateSettingsRequest {
  preferredLanguage?: Language;
  voiceName?: string;
  voiceSpeed?: number;
  voicePitch?: number;
  defaultExpression?: string;
  themePreference?: string;
  showSubtitles?: boolean;
  autoPlayAudio?: boolean;
  enabledAnimations?: string[];
  blinkEnabled?: boolean;
  blinkFrequency?: number;
  thinkingPoseEnabled?: boolean;
  avatarModelId?: string | null;
  clearAvatarModel?: boolean;
}

export interface FeatureFlagsDto {
  role: Role;
  canUseCustomApiKey: boolean;
  canAccessAllExpressions: boolean;
  canAccessAllAnimations: boolean;
  canSelectAvatarModel: boolean;
  canCustomizeVoice: boolean;
  canAccessChatHistory: boolean;
  maxConversationHistory: number;
  maxMessagesPerDay: number;
}

export interface CustomAiConfigDto {
  hasConfig: boolean;
  maskedApiKey?: string | null;
  preferredModel?: string | null;
  isVerified: boolean;
  lastTestedAt?: string | null;
}

export interface AvatarModelDto {
  id: string;
  name: string;
  fileUrl: string;
  thumbnailUrl?: string | null;
  description?: string | null;
  minRole: Role;
  isDefault: boolean;
  isActive: boolean;
}

export interface ExpressionDto {
  id: string;
  name: string;
  displayName: string;
  description?: string | null;
  minRole: Role;
  isActive: boolean;
}

export interface AnimationDto {
  id: string;
  name: string;
  displayName: string;
  description?: string | null;
  minRole: Role;
  isActive: boolean;
}

export interface SubscriptionPlanDto {
  id: string;
  name: string;
  targetRole: Role;
  billingCycle: string;
  price: number;
  currency: string;
  description?: string | null;
  features: string[];
  isActive: boolean;
}

export interface SubscriptionDto {
  id: string;
  planName?: string | null;
  startDate: string;
  endDate: string;
  status: string;
}

export interface CheckoutData {
  transactionId: string;
  provider: string;
  isDemo: boolean;
  checkoutUrl?: string | null;
  sessionData?: string | null;
}

export interface AdminUserDto {
  id: string;
  email: string;
  displayName: string;
  role: Role;
  status: string;
  createdAt: string;
  subscriptionStatus?: string | null;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface SystemSettingDto {
  key: string;
  value: string;
  description?: string | null;
  updatedAt: string;
}

export interface AuditLogDto {
  id: string;
  userId?: string | null;
  action: string;
  details?: string | null;
  createdAt: string;
}

export interface ApiErrorBody {
  error?: string;
  message?: string;
  errors?: Record<string, string[]>;
  correlationId?: string;
}
