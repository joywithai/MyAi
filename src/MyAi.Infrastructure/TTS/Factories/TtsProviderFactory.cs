using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces;
using MyAi.Infrastructure.TTS.EdgeTts;
using MyAi.Infrastructure.TTS.Shared;

namespace MyAi.Infrastructure.TTS.Factories;

/// <summary>Factory pattern: creates TTS providers by name ("edge" default).</summary>
public class TtsProviderFactory : ITtsProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<TtsProviderFactory> _logger;

    private readonly Dictionary<string, Func<ITtsProvider>> _providers;

    public TtsProviderFactory(
        IServiceProvider serviceProvider,
        EdgeTtsWebSocketClient webSocketClient,
        VoiceSelector voiceSelector,
        ILogger<TtsProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _providers = new Dictionary<string, Func<ITtsProvider>>(StringComparer.OrdinalIgnoreCase)
        {
            ["edge"] = () => new EdgeTtsAdapter(
                webSocketClient, voiceSelector, serviceProvider.GetRequiredService<ILogger<EdgeTtsAdapter>>())
        };
    }

    public ITtsProvider Create(string providerName)
    {
        var name = string.IsNullOrWhiteSpace(providerName) ? "edge" : providerName;

        if (_providers.TryGetValue(name, out var factory))
        {
            return factory();
        }

        _logger.LogWarning("Unknown TTS provider '{Provider}'; using edge", name);
        return _providers["edge"]();
    }
}
