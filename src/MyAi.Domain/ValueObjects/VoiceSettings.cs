using MyAi.Domain.Enums;
using MyAi.Domain.Exceptions;

namespace MyAi.Domain.ValueObjects;

/// <summary>Immutable TTS voice preferences. Speed 0.50-2.00, pitch -50..+50.</summary>
public sealed class VoiceSettings : IEquatable<VoiceSettings>
{
    public const float MinSpeed = 0.50f;
    public const float MaxSpeed = 2.00f;
    public const int MinPitch = -50;
    public const int MaxPitch = 50;

    public VoiceSettings(Language language, string voiceName, float speed, int pitch)
    {
        if (speed < MinSpeed || speed > MaxSpeed)
        {
            throw new DomainException($"Voice speed must be between {MinSpeed} and {MaxSpeed}, got {speed}.");
        }

        if (pitch < MinPitch || pitch > MaxPitch)
        {
            throw new DomainException($"Voice pitch must be between {MinPitch} and {MaxPitch}, got {pitch}.");
        }

        if (string.IsNullOrWhiteSpace(voiceName))
        {
            throw new DomainException("Voice name is required.");
        }

        Language = language;
        VoiceName = voiceName;
        Speed = speed;
        Pitch = pitch;
    }

    public Language Language { get; }
    public string VoiceName { get; }
    public float Speed { get; }
    public int Pitch { get; }

    public static VoiceSettings CreateDefault(Language language) => new(
        language,
        language == Language.Bn ? "bn-BD-NabanitaNeural" : "en-US-JennyNeural",
        1.00f,
        0);

    public VoiceSettings WithSpeed(float speed) => new(Language, VoiceName, speed, Pitch);

    public VoiceSettings WithPitch(int pitch) => new(Language, VoiceName, Speed, pitch);

    public bool Equals(VoiceSettings? other) =>
        other is not null && Language == other.Language && VoiceName == other.VoiceName
        && Math.Abs(Speed - other.Speed) < 0.001f && Pitch == other.Pitch;

    public override bool Equals(object? obj) => Equals(obj as VoiceSettings);

    public override int GetHashCode() => HashCode.Combine(Language, VoiceName, Speed, Pitch);
}
