namespace MyAi.Application.Common.Exceptions;

/// <summary>Validation failure raised from the pipeline or handlers → HTTP 400.</summary>
public class ValidationException : Exception
{
    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToArray();
    }

    public ValidationException(string error)
        : base("One or more validation errors occurred.")
    {
        Errors = new[] { error };
    }

    public IReadOnlyList<string> Errors { get; }
}

/// <summary>Resource not found → HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' ({key}) was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>Authentication failure → HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Authentication failed.") : base(message)
    {
    }
}

/// <summary>Authorization failure → HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You do not have access to this resource.") : base(message)
    {
    }
}

/// <summary>Conflict (duplicate resource) → HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>Rate limit exceeded → HTTP 429.</summary>
public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message = "Rate limit exceeded. Please try again later.")
        : base(message)
    {
        RetryAfterSeconds = 60;
    }

    public RateLimitExceededException(string message, int retryAfterSeconds) : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; }
}

/// <summary>External dependency failure (AI, TTS, storage) → HTTP 502/503.</summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string service, string message)
        : base($"External service '{service}' failed: {message}")
    {
        Service = service;
    }

    public string Service { get; }
}
