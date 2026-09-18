using System.Text.Json;
using MyAi.Domain.Enums;
using MyAi.Domain.Events;
using MyAi.Domain.Events.Base;
using MyAi.Domain.Interfaces;
using MyAi.Domain.ValueObjects;

namespace MyAi.Domain.Entities;

public class UserSettings : Entity, IAuditableEntity
{
    private static readonly List<string> PublicDefaultAnimations = new() { "breathing" };

    private UserSettings() { } // EF Core

    private UserSettings(Guid userId)
    {
        UserId = userId;
        PreferredLanguage = Language.Bn;
        VoiceName = "bn-BD-NabanitaNeural";
        VoiceSpeed = 1.00f;
        VoicePitch = 0;
        DefaultExpression = "FRIENDLY";
        ThemePreference = "dark";
        ShowSubtitles = true;
        AutoPlayAudio = true;
        EnabledAnimations = new List<string>(PublicDefaultAnimations);
        BlinkEnabled = true;
        BlinkFrequency = "normal";
        ThinkingPoseEnabled = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public Guid? AvatarModelId { get; private set; }

    public Language PreferredLanguage { get; private set; }

    public string VoiceName { get; private set; }

    public float VoiceSpeed { get; private set; }

    public int VoicePitch { get; private set; }

    public string DefaultExpression { get; private set; }

    public string ThemePreference { get; private set; }

    public bool ShowSubtitles { get; private set; }

    public bool AutoPlayAudio { get; private set; }

    public List<string> EnabledAnimations { get; private set; }

    public bool BlinkEnabled { get; private set; }

    public string BlinkFrequency { get; private set; }

    public bool ThinkingPoseEnabled { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static UserSettings CreateDefault(Guid userId) => new(userId);

    public void UpdateVoiceSettings(VoiceSettings voiceSettings)
    {
        PreferredLanguage = voiceSettings.Language;
        VoiceName = voiceSettings.VoiceName;
        VoiceSpeed = voiceSettings.Speed;
        VoicePitch = voiceSettings.Pitch;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(
            UserId, new[] { "language", "voiceName", "voiceSpeed", "voicePitch" }));
    }

    public void UpdateAnimationSettings(AnimationSettings animationSettings)
    {
        BlinkEnabled = animationSettings.BlinkEnabled;
        BlinkFrequency = animationSettings.BlinkFrequency;
        ThinkingPoseEnabled = animationSettings.ThinkingPoseEnabled;
        EnabledAnimations = animationSettings.EnabledAnimations.ToList();
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(
            UserId, new[] { "blink", "thinkingPose", "enabledAnimations" }));
    }

    public void UpdateLanguage(Language language)
    {
        PreferredLanguage = language;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(UserId, new[] { "language" }));
    }

    public void UpdateTheme(string theme)
    {
        ThemePreference = theme;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(UserId, new[] { "theme" }));
    }

    public void UpdateDefaultExpression(string expression)
    {
        DefaultExpression = ExpressionSegment.NormalizeExpression(expression);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(UserId, new[] { "defaultExpression" }));
    }

    public void UpdateDisplayPreferences(bool showSubtitles, bool autoPlayAudio)
    {
        ShowSubtitles = showSubtitles;
        AutoPlayAudio = autoPlayAudio;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(UserId, new[] { "showSubtitles", "autoPlayAudio" }));
    }

    public void SelectAvatarModel(Guid? avatarModelId)
    {
        AvatarModelId = avatarModelId;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(UserId, new[] { "avatarModel" }));
    }

    public void UpdateEnabledAnimations(List<string> animations)
    {
        var unique = animations.Distinct().ToList();
        EnabledAnimations = unique.Count == 0 ? new List<string>(PublicDefaultAnimations) : unique;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SettingsChangedEvent(UserId, new[] { "enabledAnimations" }));
    }
}
