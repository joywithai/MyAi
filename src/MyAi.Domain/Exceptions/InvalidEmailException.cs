namespace MyAi.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email)
        : base($"Invalid email address: '{email}'.")
    {
        Email = email;
    }

    public string Email { get; }
}
