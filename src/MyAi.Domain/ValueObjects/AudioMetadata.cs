namespace MyAi.Domain.ValueObjects;

/// <summary>Word timing data used for lip-sync on the client.</summary>
public sealed class WordBoundary : IEquatable<WordBoundary>
{
    public WordBoundary(string word, int startTimeMs, int durationMs, int textOffset, int wordLength)
    {
        Word = word;
        StartTimeMs = startTimeMs;
        DurationMs = durationMs;
        TextOffset = textOffset;
        WordLength = wordLength;
    }

    public string Word { get; }
    public int StartTimeMs { get; }
    public int DurationMs { get; }
    public int TextOffset { get; }
    public int WordLength { get; }

    public bool Equals(WordBoundary? other) =>
        other is not null && StartTimeMs == other.StartTimeMs && Word == other.Word;

    public override bool Equals(object? obj) => Equals(obj as WordBoundary);

    public override int GetHashCode() => HashCode.Combine(Word, StartTimeMs);
}

/// <summary>Immutable audio metadata attached to assistant messages.</summary>
public sealed class AudioMetadata : IEquatable<AudioMetadata>
{
    public AudioMetadata(
        string audioUrl,
        int durationMs,
        string contentType,
        IReadOnlyList<WordBoundary> wordBoundaries)
    {
        AudioUrl = audioUrl;
        DurationMs = durationMs;
        ContentType = contentType;
        WordBoundaries = wordBoundaries;
    }

    public string AudioUrl { get; }
    public int DurationMs { get; }
    public string ContentType { get; }
    public IReadOnlyList<WordBoundary> WordBoundaries { get; }

    public static AudioMetadata Create(
        string audioUrl, int durationMs, string contentType, IReadOnlyList<WordBoundary> wordBoundaries)
        => new(audioUrl, durationMs, contentType, wordBoundaries);

    public bool Equals(AudioMetadata? other) =>
        other is not null && AudioUrl == other.AudioUrl && DurationMs == other.DurationMs;

    public override bool Equals(object? obj) => Equals(obj as AudioMetadata);

    public override int GetHashCode() => HashCode.Combine(AudioUrl, DurationMs);
}
