using Serilog.Core;
using Serilog.Events;

namespace MyAi.Infrastructure.Logging;

/// <summary>Adds application/environment/correlation-id context to every log entry.</summary>
public class LogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Application", "MyAi"));

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", environment));

        // Correlation ID is set per-request by CorrelationIdMiddleware via LogContext.
    }
}
