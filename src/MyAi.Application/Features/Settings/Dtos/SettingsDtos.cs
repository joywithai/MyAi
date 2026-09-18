namespace MyAi.Application.Features.Settings;

public record UserSettingsDto(
    string PreferredLanguage,
    string VoiceName,
    float VoiceSpeed,
    int VoicePitch,
    string DefaultExpression,
    string ThemePreference,
    bool ShowSubtitles,
    bool AutoPlayAudio,
    List<string> EnabledAnimations,
    bool BlinkEnabled,
    string BlinkFrequency,
    bool ThinkingPoseEnabled,
    Guid? SelectedAvatarModelId);

public record RoleFeatureFlagsDto(
    string Role,
    bool CanUseCustomApiKey,
    bool CanAccessAllExpressions,
    bool CanAccessAllAnimations,
    bool CanSelectAvatarModel,
    bool CanCustomizeVoice,
    bool CanAccessChatHistory,
    int MaxConversationHistory,
    int MaxMessagesPerDay);
