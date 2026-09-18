using System.Text.Json;
using MyAi.Application.Common.Exceptions;

namespace MyAi.Api.Middleware;

/// <summary>
/// Chain of Responsibility: central unhandled-exception boundary.
/// Maps application exceptions to HTTP status codes and returns a consistent error JSON.
/// Stack traces are hidden outside development.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;

    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = MapExceptionToStatusCode(exception);
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);

        if (statusCode == 500)
        {
            _logger.LogError(exception, "Unhandled exception ({CorrelationId}): {Message}", correlationId, exception.Message);
        }
        else if (statusCode != 499)
        {
            _logger.LogWarning("Handled exception ({CorrelationId}): {Message}", correlationId, exception.Message);
        }

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode == 499 ? 200 : statusCode; // 499 is not a valid wire status
        context.Response.ContentType = "application/json";

        if (exception is RateLimitExceededException rateLimited)
        {
            context.Response.Headers.RetryAfter = rateLimited.RetryAfterSeconds.ToString();
        }

        var message = exception switch
        {
            ValidationException validationEx => string.Join("; ", validationEx.Errors),
            NotFoundException notFound => notFound.Message,
            UnauthorizedException unauthorizedEx => unauthorizedEx.Message,
            ForbiddenException forbiddenEx => forbiddenEx.Message,
            ConflictException conflictEx => conflictEx.Message,
            ExternalServiceException => "An external service is temporarily unavailable. Please try again shortly.",
            OperationCanceledException => "Request was cancelled.",
            _ => "An unexpected error occurred. Please try again later."
        };

        var errorResponse = CreateErrorResponse(message, correlationId, exception);

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }

    private static int MapExceptionToStatusCode(Exception exception) => exception switch
    {
        NotFoundException => StatusCodes.Status404NotFound,
        ValidationException => StatusCodes.Status400BadRequest,
        UnauthorizedException => StatusCodes.Status401Unauthorized,
        ForbiddenException => StatusCodes.Status403Forbidden,
        ConflictException => StatusCodes.Status409Conflict,
        RateLimitExceededException => StatusCodes.Status429TooManyRequests,
        ExternalServiceException => StatusCodes.Status502BadGateway,
        OperationCanceledException => 499, // client closed request (nginx convention)
        _ => StatusCodes.Status500InternalServerError
    };

    private static object CreateErrorResponse(string message, string correlationId, Exception exception)
    {
        var errors = exception is ValidationException validationException
            ? validationException.Errors
            : null;

        return new
        {
            error = true,
            message,
            errors,
            correlationId,
            timestamp = DateTime.UtcNow
        };
    }
}
