using MyAi.Infrastructure.TTS.EdgeTts.Models;

namespace MyAi.Infrastructure.TTS.Shared;

/// <summary>Audio data conversion utilities.</summary>
public static class AudioConverter
{
    public static string ToBase64(byte[] audioBytes) => Convert.ToBase64String(audioBytes);

    /// <summary>Total duration = end of the last word boundary (0 when none).</summary>
    public static int CalculateDurationMs(List<WordBoundaryData> boundaries)
    {
        if (boundaries is null || boundaries.Count == 0)
        {
            return 0;
        }

        var last = boundaries.Max(b => b.StartTimeMs + b.DurationMs);
        return last;
    }

    public static string NormalizeContentType(string contentType) =>
        string.IsNullOrWhiteSpace(contentType) ? "audio/mpeg"
        : contentType.Contains("mpeg", StringComparison.OrdinalIgnoreCase) ? "audio/mpeg"
        : contentType.Contains("ogg", StringComparison.OrdinalIgnoreCase) ? "audio/ogg"
        : contentType.Contains("wav", StringComparison.OrdinalIgnoreCase) ? "audio/wav"
        : contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ? contentType
        : "audio/mpeg";
}
