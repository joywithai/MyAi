using MyAi.Application.Common.Ai;
using MyAi.Domain.ValueObjects;

namespace MyAi.Infrastructure.AI.Shared;

/// <summary>
/// Single source of truth for the 12 supported expressions.
/// AI output is normalized through here before it ever reaches the database or frontend.
/// </summary>
public static class ExpressionValidator
{
    public static readonly IReadOnlyList<string> SupportedExpressions = new[]
    {
        "NEUTRAL", "HAPPY", "SAD", "ANGRY", "SURPRISED", "RELAXED",
        "EXCITED", "CONFUSED", "THOUGHTFUL", "CONCERNED", "FRIENDLY", "SERIOUS"
    };

    public static bool IsSupported(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        return System.Enum.TryParse<Domain.Enums.ExpressionType>(
            expression.Trim(), ignoreCase: true, out _);
    }

    public static string Normalize(string? expression) =>
        IsSupported(expression) ? expression!.Trim().ToUpperInvariant() : "NEUTRAL";

    public static IReadOnlyList<string> GetSupportedExpressions() => SupportedExpressions;

    public static bool IsSafeText(string? text) =>
        !string.IsNullOrWhiteSpace(text) && text.Trim().Length > 0;

    /// <summary>Cleans raw segments: valid text only, expressions normalized, empties dropped.</summary>
    public static List<ExpressionSegment> NormalizeSegments(IEnumerable<ExpressionSegment>? segments)
    {
        if (segments is null)
        {
            return new List<ExpressionSegment>();
        }

        var normalized = segments
            .Where(s => IsSafeText(s.Text))
            .Select(s => ExpressionSegment.Create(Normalize(s.Expression), s.Text))
            .ToList();

        return normalized;
    }
}
