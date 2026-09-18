using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Application.Common.Tts;

/// <summary>Input for a TTS synthesis call.</summary>
public class TtsRequest
{
    public required string Text { get; init; }

    public required Language Language { get; init; }

    public VoiceSettings VoiceSettings { get; init; } = VoiceSettings.CreateDefault(Language.Bn);
}

/// <summary>Normalized output of a TTS synthesis call.</summary>
public class TtsResult
{
    public required byte[] Audio { get; init; }

    public required string ContentType { get; init; }

    public required int DurationMs { get; init; }

    public List<WordBoundary> WordBoundaries { get; init; } = new();

    public required string VoiceUsed { get; init; }

    public bool IsEmpty => Audio.Length == 0;
}
