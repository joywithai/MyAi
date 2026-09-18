using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MyAi.Infrastructure.Logging;

/// <summary>Central Serilog setup used by Program.cs (UseSerilog with this configurator).</summary>
public static class SerilogConfiguration
{
    public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
        (context, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Is(context.HostingEnvironment.IsDevelopment()
                    ? LogEventLevel.Debug
                    : LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With<LogEnricher>()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    Path.Combine("logs", "myai-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

            // Sensitive data masking: password/apiKey properties never logged.
            loggerConfiguration.Enrich.WithProperty("Application", "MyAi");
        };
}
