namespace MyAi.Api.Contracts;

public class UpdateSettingsRequest
{
    public string? PreferredLanguage { get; set; }

    public string? VoiceName { get; set; }

    public float? VoiceSpeed { get; set; }

    public int? VoicePitch { get; set; }

    public string? DefaultExpression { get; set; }

    public string? ThemePreference { get; set; }

    public bool? ShowSubtitles { get; set; }

    public bool? AutoPlayAudio { get; set; }

    public List<string>? EnabledAnimations { get; set; }

    public bool? BlinkEnabled { get; set; }

    public string? BlinkFrequency { get; set; }

    public bool? ThinkingPoseEnabled { get; set; }

    public Guid? AvatarModelId { get; set; }

    public bool? ClearAvatarModel { get; set; }
}

public class CustomAiConfigRequest
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    public string? PreferredModel { get; set; }
}
