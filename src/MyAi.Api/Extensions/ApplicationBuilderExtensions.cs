using MyAi.Api.Middleware;

namespace MyAi.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>Order: correlationId → request logging → exception handler → security headers
    /// → rate limiting → CORS → static files → auth → authorization → endpoints.</summary>
    public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseSecurityHeaders();
        app.UseRateLimiting();
        app.UseCors("Web");

        app.UseStaticFiles(); // local storage uploads (wwwroot)

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next();
        });

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app) =>
        app.UseMiddleware<RateLimitingMiddleware>();

    public static IApplicationBuilder UseSwaggerInDevelopment(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyAi API v1");
                options.DocumentTitle = "MyAi API";
            });
        }

        return app;
    }
}
