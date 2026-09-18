namespace MyAi.Api.Middleware;

/// <summary>Assigns a unique correlation ID to every request (header X-Correlation-ID).</summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }

    public static string GetCorrelationId(HttpContext context) =>
        context.Items.TryGetValue(HeaderName, out var value) && value is string id
            ? id
            : context.Response.Headers[HeaderName].FirstOrDefault() ?? "unknown";
}
