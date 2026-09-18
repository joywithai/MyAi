namespace MyAi.Infrastructure.AI.OpenRouter;

/// <summary>Tests OpenRouter API keys (used by the custom AI config flow).</summary>
public class OpenRouterKeyTester : IOpenRouterKeyTester
{
    private readonly OpenRouterHttpClient _httpClient;

    public OpenRouterKeyTester(OpenRouterHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool IsValid, string? ErrorDetail, string? ModelInfo)> TestKeyAsync(
        string apiKey, string? model, CancellationToken ct = default)
    {
        var (isValid, errorDetail) = await _httpClient.TestKeyAsync(apiKey, ct);
        return (isValid, errorDetail, model);
    }
}
