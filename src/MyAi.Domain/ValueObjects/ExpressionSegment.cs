using MyAi.Domain.Exceptions;

namespace MyAi.Domain.ValueObjects;

/// <summary>Immutable expression + text segment produced by the AI response parser.</summary>
public sealed class ExpressionSegment : IEquatable<ExpressionSegment>
{
    public ExpressionSegment(string expression, string text)
    {
        Expression = expression;
        Text = text;
    }

    public string Expression { get; }

    public string Text { get; }

    /// <summary>Creates a segment, normalising invalid expressions to NEUTRAL.</summary>
    public static ExpressionSegment Create(string expression, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidExpressionException(expression ?? string.Empty);
        }

        var normalized = NormalizeExpression(expression);

        return new ExpressionSegment(normalized, text.Trim());
    }

    public static string NormalizeExpression(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return "NEUTRAL";
        }

        var normalized = expression.Trim().ToUpperInvariant();

        return System.Enum.TryParse<ExpressionType>(normalized, ignoreCase: true, out _)
            ? normalized
            : "NEUTRAL";
    }

    public bool Equals(ExpressionSegment? other) =>
        other is not null && Expression == other.Expression && Text == other.Text;

    public override bool Equals(object? obj) => Equals(obj as ExpressionSegment);

    public override int GetHashCode() => HashCode.Combine(Expression, Text);
}
