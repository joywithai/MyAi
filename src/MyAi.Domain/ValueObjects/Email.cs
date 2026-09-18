using MyAi.Domain.Exceptions;

namespace MyAi.Domain.ValueObjects;

/// <summary>Immutable email value object with format validation.</summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly System.Text.RegularExpressions.Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidEmailException(email ?? string.Empty);
        }

        var normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > 255 || !EmailRegex.IsMatch(normalized))
        {
            throw new InvalidEmailException(email);
        }

        return new Email(normalized);
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
