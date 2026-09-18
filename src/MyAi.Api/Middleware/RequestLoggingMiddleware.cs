using System.Diagnostics;

namespace MyAi.Api.Middleware;

/// <summary>Structured request/response logging with duration (warn &gt; 2000ms).</summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await _next(context);
        }
        finally
        {
            var durationMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            await LogRequestAsync(context, durationMs);
        }
    }

    private Task LogRequestAsync(HttpContext context, double durationMs)
    {
        var userId = GetUserId(context);
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);

        var logLevel = durationMs > 2000 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(
            logLevel,
            "HTTP {Method} {Path} → {StatusCode} in {Duration:F0}ms (user={UserId}, ip={Ip}, correlation={CorrelationId})",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            durationMs,
            userId ?? "anonymous",
            context.Connection.RemoteIpAddress?.ToString() ?? "-",
            correlationId);

        return Task.CompletedTask;
    }

    private static string? GetUserId(HttpContext context) =>
        context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? context.User?.FindFirst("sub")?.Value;
}
