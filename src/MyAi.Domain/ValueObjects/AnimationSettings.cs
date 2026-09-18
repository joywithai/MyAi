namespace MyAi.Domain.ValueObjects;

/// <summary>Immutable avatar animation preferences.</summary>
public sealed class AnimationSettings : IEquatable<AnimationSettings>
{
    private static readonly IReadOnlyList<string> DefaultAnimations = new[] { "breathing" };

    public AnimationSettings(
        bool blinkEnabled,
        string blinkFrequency,
        bool thinkingPoseEnabled,
        IReadOnlyList<string> enabledAnimations)
    {
        var validFrequencies = new[] { "low", "normal", "high" };

        if (!validFrequencies.Contains(blinkFrequency ?? string.Empty))
        {
            throw new ArgumentException($"Blink frequency must be one of: {string.Join(", ", validFrequencies)}.");
        }

        BlinkEnabled = blinkEnabled;
        BlinkFrequency = blinkFrequency;
        ThinkingPoseEnabled = thinkingPoseEnabled;
        EnabledAnimations = enabledAnimations.Count == 0 ? DefaultAnimations : enabledAnimations;
    }

    public bool BlinkEnabled { get; }
    public string BlinkFrequency { get; }
    public bool ThinkingPoseEnabled { get; }
    public IReadOnlyList<string> EnabledAnimations { get; }

    public static AnimationSettings CreateDefault() => new(
        blinkEnabled: true,
        blinkFrequency: "normal",
        thinkingPoseEnabled: true,
        DefaultAnimations.ToList());

    public AnimationSettings WithBlinkEnabled(bool enabled) =>
        new(enabled, BlinkFrequency, ThinkingPoseEnabled, EnabledAnimations);

    public bool Equals(AnimationSettings? other) =>
        other is not null
        && BlinkEnabled == other.BlinkEnabled
        && BlinkFrequency == other.BlinkFrequency
        && ThinkingPoseEnabled == other.ThinkingPoseEnabled
        && EnabledAnimations.SequenceEqual(other.EnabledAnimations);

    public override bool Equals(object? obj) => Equals(obj as AnimationSettings);

    public override int GetHashCode() => HashCode.Combine(BlinkEnabled, BlinkFrequency, ThinkingPoseEnabled);
}
