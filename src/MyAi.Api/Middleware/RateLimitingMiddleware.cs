using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Api.Middleware;

/// <summary>
/// Redis-backed sliding window rate limiting.
/// Chat endpoint: public_user 50/day, subscriber 500/day, admin unlimited.
/// Other endpoints: anonymous 10/minute default.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ICacheService _cacheService;

    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next, ICacheService cacheService, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsRateLimitedEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var role = context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                   ?? context.User?.FindFirst("role")?.Value;

        var (limit, window) = GetRateLimitForRole(role);
        var key = BuildRateLimitKey(context);

        var (allowed, remaining, resetAt) = await TryConsumeAsync(key, limit, window);

        AddRateLimitHeaders(context, limit, remaining, resetAt);

        if (!allowed)
        {
            _logger.LogWarning("Rate limit exceeded for {Key} on {Path}", key, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.RetryAfter = ((int)(resetAt - DateTimeOffset.UtcNow).TotalSeconds).ToString();
            await context.Response.WriteAsync(
                "{\"error\":true,\"message\":\"Rate limit exceeded. Please try again later.\"}");
            return;
        }

        await _next(context);
    }

    private static string BuildRateLimitKey(HttpContext context)
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? context.User?.FindFirst("sub")?.Value;

        var identity = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ratelimit:{identity}:{context.Request.Path.Value?.ToLowerInvariant()}";
    }

    private static (int Limit, TimeSpan Window) GetRateLimitForRole(string? role) => role switch
    {
        RoleConstants.Admin => (int.MaxValue, TimeSpan.FromDays(1)),   // unlimited
        RoleConstants.Subscriber => (500, TimeSpan.FromDays(1)),
        RoleConstants.PublicUser => (50, TimeSpan.FromDays(1)),
        _ => (10, TimeSpan.FromMinutes(1))                              // anonymous default
    };

    private async Task<(bool Allowed, int Remaining, DateTimeOffset ResetAt)> TryConsumeAsync(
        string key, int limit, TimeSpan window)
    {
        if (limit == int.MaxValue)
        {
            return (true, int.MaxValue, DateTimeOffset.UtcNow.Add(window));
        }

        try
        {
            var cacheKey = $"{key}:{DateTimeOffset.UtcNow.UtcTicks / window.Ticks}";

            var current = await _cacheService.GetAsync<CounterValue>(cacheKey) ?? new CounterValue(0);
            var count = current.Count + 1;

            if (count > limit)
            {
                return (false, 0, DateTimeOffset.UtcNow.Add(window));
            }

            await _cacheService.SetAsync(cacheKey, new CounterValue(count), window);

            return (true, Math.Max(0, limit - count), DateTimeOffset.UtcNow.Add(window));
        }
        catch (Exception ex)
        {
            // Graceful degradation: cache down → allow the request.
            _logger.LogWarning(ex, "Rate limiter cache failure; allowing request");
            return (true, limit, DateTimeOffset.UtcNow.Add(window));
        }
    }

    private static void AddRateLimitHeaders(HttpContext context, int limit, int remaining, DateTimeOffset resetAt)
    {
        context.Response.Headers["X-RateLimit-Limit"] =
            limit == int.MaxValue ? "unlimited" : limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] =
            remaining == int.MaxValue ? "unlimited" : remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = resetAt.ToUnixTimeSeconds().ToString();
    }

    private static bool IsRateLimitedEndpoint(PathString path) =>
        path.StartsWithSegments("/api/v1/chat");

    private sealed record CounterValue(int Count);
}
