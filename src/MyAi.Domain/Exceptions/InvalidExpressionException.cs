namespace MyAi.Domain.Exceptions;

public class InvalidExpressionException : DomainException
{
    public InvalidExpressionException(string expression)
        : base($"Invalid expression: '{expression}'.")
    {
        Expression = expression;
    }

    public string Expression { get; }
}
