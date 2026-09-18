using MyAi.Domain.ValueObjects;

namespace MyAi.Infrastructure.TTS.EdgeTts.Models;

/// <summary>Edge TTS synthesis request.</summary>
public class EdgeTtsRequest
{
    public required string Text { get; init; }

    public required string Voice { get; init; }

    /// <summary>Rate adjustment, e.g. "+0%" or "-25%".</summary>
    public string Rate { get; init; } = "+0%";

    /// <summary>Pitch adjustment, e.g. "+0Hz" or "+10Hz".</summary>
    public string Pitch { get; init; } = "+0Hz";

    public string OutputFormat { get; init; } = "audio-24khz-48kbitrate-mono-mp3";
}

/// <summary>Edge TTS synthesis result.</summary>
public class EdgeTtsResponse
{
    public byte[] Audio { get; init; } = Array.Empty<byte>();

    public List<WordBoundaryData> WordBoundaries { get; init; } = new();

    public string ContentType { get; init; } = "audio/mpeg";

    public bool IsSuccess => Audio.Length > 0;

    public string? ErrorMessage { get; init; }
}

/// <summary>Word timing info emitted by Edge TTS (ticks → ms converted by the client).</summary>
public class WordBoundaryData
{
    public required string Word { get; init; }

    public required int StartTimeMs { get; init; }

    public required int DurationMs { get; init; }

    public int TextOffset { get; init; }

    public int WordLength { get; init; }
}
