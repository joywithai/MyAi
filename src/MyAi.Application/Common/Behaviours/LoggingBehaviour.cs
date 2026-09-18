using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MyAi.Application.Common.Behaviours;

/// <summary>MediatR pipeline behaviour: logs request name, duration and outcome.</summary>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startTimestamp = Stopwatch.GetTimestamp();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            _logger.LogInformation("Handled {RequestName} in {ElapsedMs:F1}ms", requestName, elapsedMs);

            if (elapsedMs > 2000)
            {
                _logger.LogWarning("Slow handler {RequestName} took {ElapsedMs:F1}ms", requestName, elapsedMs);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}: {Message}", requestName, ex.Message);
            throw;
        }
    }
}
