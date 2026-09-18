using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Application.Common.Ai;

/// <summary>A single prior chat turn for conversation context.</summary>
public record ChatHistoryItem(string Role, string Content);

/// <summary>Input for an AI provider call.</summary>
public class AiRequest
{
    public required string Message { get; init; }

    public required Language Language { get; init; }

    /// <summary>Expressions the user is allowed to use (role-based).</summary>
    public IReadOnlyList<string> AccessibleExpressions { get; init; } = Array.Empty<string>();

    /// <summary>Recent conversation turns for context (oldest first).</summary>
    public IReadOnlyList<ChatHistoryItem> History { get; init; } = Array.Empty<ChatHistoryItem>();

    /// <summary>Preferred AI model identifier (user custom model or system default).</summary>
    public string? PreferredModel { get; init; }

    /// <summary>API key for this call. Null means use the system key.</summary>
    public string? ApiKey { get; init; }
}

/// <summary>Normalized output of an AI provider call.</summary>
public class AiResult
{
    public required string ReplyText { get; init; }

    public string? Script { get; init; }

    public List<ExpressionSegment> Segments { get; init; } = new();

    public string? ModelUsed { get; init; }

    public int? TokensUsed { get; init; }

    public string Language { get; init; } = "bn";

    public static AiResult Fallback(string rawText, string language) => new()
    {
        ReplyText = rawText,
        Segments = new List<ExpressionSegment> { new("NEUTRAL", rawText) },
        Language = language
    };
}
