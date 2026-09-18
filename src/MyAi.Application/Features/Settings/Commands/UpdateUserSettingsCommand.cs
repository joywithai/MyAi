using MediatR;
using MyAi.Application.Features.Settings;

namespace MyAi.Application.Features.Settings.Commands;

/// <summary>Partial settings update — null fields are left untouched.</summary>
public record UpdateUserSettingsCommand(
    string? PreferredLanguage = null,
    string? VoiceName = null,
    float? VoiceSpeed = null,
    int? VoicePitch = null,
    string? DefaultExpression = null,
    string? ThemePreference = null,
    bool? ShowSubtitles = null,
    bool? AutoPlayAudio = null,
    List<string>? EnabledAnimations = null,
    bool? BlinkEnabled = null,
    string? BlinkFrequency = null,
    bool? ThinkingPoseEnabled = null,
    Guid? AvatarModelId = null,
    bool? ClearAvatarModel = null) : IRequest<UserSettingsDto>;
