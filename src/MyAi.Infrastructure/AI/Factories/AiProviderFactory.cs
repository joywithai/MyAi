using Microsoft.Extensions.Logging;
using MyAi.Application.Common.Interfaces;
using MyAi.Infrastructure.AI.Demo;
using MyAi.Infrastructure.AI.OpenRouter;

namespace MyAi.Infrastructure.AI.Factories;

/// <summary>
/// Factory pattern: creates AI providers by name. A custom key produces a
/// provider bound to that key; otherwise the system-configured provider is used.
/// When no system key is configured, the demo provider keeps the app usable.
/// </summary>
public class AiProviderFactory : IAiProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    private readonly string? _systemApiKey;

    private readonly string _systemDefaultModel;

    private readonly ILogger<AiProviderFactory> _logger;

    private readonly Dictionary<string, Func<string?, IAiProvider>> _registeredProviders;

    public AiProviderFactory(
        IServiceProvider serviceProvider,
        OpenRouterHttpClient openRouterHttpClient,
        OpenRouterPromptBuilder promptBuilder,
        OpenRouterResponseParser responseParser,
        string? systemApiKey,
        string systemDefaultModel,
        ILogger<AiProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _systemApiKey = systemApiKey;
        _systemDefaultModel = systemDefaultModel;
        _logger = logger;

        _registeredProviders = new Dictionary<string, Func<string?, IAiProvider>>(StringComparer.OrdinalIgnoreCase)
        {
            ["openrouter"] = key => new OpenRouterAiProvider(
                openRouterHttpClient, promptBuilder, responseParser, key ?? _systemApiKey, _systemDefaultModel),
            ["demo"] = _ => new DemoAiProvider()
        };
    }

    public IAiProvider Create(string providerName, string? customApiKey = null)
    {
        var effectiveName = string.IsNullOrWhiteSpace(providerName) ? "openrouter" : providerName;

        if (!_registeredProviders.TryGetValue(effectiveName, out var factory))
        {
            _logger.LogWarning("Unknown AI provider '{Provider}'; falling back to openrouter", effectiveName);
            factory = _registeredProviders["openrouter"];
        }

        var provider = factory(customApiKey);

        // Graceful degradation: no key anywhere → demo provider, never a crash.
        if (provider is OpenRouterAiProvider && string.IsNullOrWhiteSpace(customApiKey) && string.IsNullOrWhiteSpace(_systemApiKey))
        {
            _logger.LogInformation("No AI API key configured; using demo provider");
            return _registeredProviders["demo"](null);
        }

        return provider;
    }

    /// <summary>Extension point: register additional providers at startup.</summary>
    public void RegisterProvider(string name, Func<string?, IAiProvider> factory) =>
        _registeredProviders[name] = factory;
}
