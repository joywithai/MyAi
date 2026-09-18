using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Settings;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Application.Features.Settings.Commands;

public class UpdateUserSettingsCommandHandler : IRequestHandler<UpdateUserSettingsCommand, UserSettingsDto>
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IFeatureFlagsRepository _featureFlagsRepository;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateUserSettingsCommandHandler(
        ISettingsRepository settingsRepository,
        IFeatureFlagsRepository featureFlagsRepository,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _featureFlagsRepository = featureFlagsRepository;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserSettingsDto> Handle(UpdateUserSettingsCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var settings = await _settingsRepository.GetByUserIdAsync(userId, ct)
                       ?? UserSettings.CreateDefault(userId);

        var role = RoleConstants.ToUserRole(_currentUserService.GetUserRole());
        var flags = await _featureFlagsRepository.GetByRoleAsync(role, ct)
                    ?? RoleFeatureFlags.CreateDefault(role);

        ValidateFeatureAccess(command, flags);
        ApplyChanges(settings, command);

        await _settingsRepository.UpdateAsync(settings, ct);
        await _cacheService.RemoveAsync(CacheKeys.Settings(userId), ct);

        return _mapper.Map<UserSettingsDto>(settings);
    }

    /// <summary>Rejects changes to locked (subscriber-only) settings with 403.</summary>
    private static void ValidateFeatureAccess(UpdateUserSettingsCommand command, RoleFeatureFlags flags)
    {
        if (!flags.CanCustomizeVoice && (command.VoiceSpeed.HasValue || command.VoicePitch.HasValue))
        {
            throw new ForbiddenException("Voice speed/pitch customization requires a subscription.");
        }

        if (!flags.CanCustomizeVoice && !string.IsNullOrWhiteSpace(command.VoiceName))
        {
            throw new ForbiddenException("Voice selection requires a subscription.");
        }

        if (!flags.CanSelectAvatarModel && (command.AvatarModelId.HasValue || command.ClearAvatarModel == true))
        {
            throw new ForbiddenException("Avatar model selection requires a subscription.");
        }

        if (!flags.CanAccessAllAnimations && command.EnabledAnimations is { Count: > 0 })
        {
            var premiumRequested = command.EnabledAnimations
                .Where(a => !string.Equals(a, "breathing", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (premiumRequested.Count > 0)
            {
                throw new ForbiddenException("Premium animations require a subscription.");
            }
        }
    }

    /// <summary>Applies only the non-null fields (partial update).</summary>
    private static void ApplyChanges(UserSettings settings, UpdateUserSettingsCommand command)
    {
        if (!string.IsNullOrWhiteSpace(command.PreferredLanguage))
        {
            settings.UpdateLanguage(command.PreferredLanguage.Equals("en", StringComparison.OrdinalIgnoreCase)
                ? Language.En
                : Language.Bn);
        }

        if (!string.IsNullOrWhiteSpace(command.VoiceName))
        {
            settings.UpdateVoiceSettings(new VoiceSettings(
                settings.PreferredLanguage, command.VoiceName, settings.VoiceSpeed, settings.VoicePitch));
        }

        if (command.VoiceSpeed.HasValue)
        {
            settings.UpdateVoiceSettings(settings.VoiceSpeed == command.VoiceSpeed.Value
                ? new VoiceSettings(settings.PreferredLanguage, settings.VoiceName, command.VoiceSpeed.Value, settings.VoicePitch)
                : new VoiceSettings(settings.PreferredLanguage, settings.VoiceName, command.VoiceSpeed.Value, settings.VoicePitch));
        }

        if (command.VoicePitch.HasValue)
        {
            settings.UpdateVoiceSettings(new VoiceSettings(
                settings.PreferredLanguage, settings.VoiceName, settings.VoiceSpeed, command.VoicePitch.Value));
        }

        if (!string.IsNullOrWhiteSpace(command.DefaultExpression))
        {
            settings.UpdateDefaultExpression(command.DefaultExpression);
        }

        if (!string.IsNullOrWhiteSpace(command.ThemePreference))
        {
            settings.UpdateTheme(command.ThemePreference);
        }

        if (command.ShowSubtitles.HasValue || command.AutoPlayAudio.HasValue)
        {
            settings.UpdateDisplayPreferences(
                command.ShowSubtitles ?? settings.ShowSubtitles,
                command.AutoPlayAudio ?? settings.AutoPlayAudio);
        }

        if (command.EnabledAnimations is { Count: > 0 })
        {
            settings.UpdateEnabledAnimations(command.EnabledAnimations);
        }

        if (command.BlinkEnabled.HasValue
            || !string.IsNullOrWhiteSpace(command.BlinkFrequency)
            || command.ThinkingPoseEnabled.HasValue)
        {
            settings.UpdateAnimationSettings(new AnimationSettings(
                command.BlinkEnabled ?? settings.BlinkEnabled,
                command.BlinkFrequency ?? settings.BlinkFrequency,
                command.ThinkingPoseEnabled ?? settings.ThinkingPoseEnabled,
                settings.EnabledAnimations));
        }

        if (command.ClearAvatarModel == true)
        {
            settings.SelectAvatarModel(null);
        }
        else if (command.AvatarModelId.HasValue)
        {
            settings.SelectAvatarModel(command.AvatarModelId.Value);
        }
    }
}
