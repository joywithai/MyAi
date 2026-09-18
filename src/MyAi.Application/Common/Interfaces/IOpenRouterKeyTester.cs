namespace MyAi.Application.Common.Interfaces;

/// <summary>Tests whether an OpenRouter API key is valid (used by the custom AI config flow).</summary>
public interface IOpenRouterKeyTester
{
    Task<(bool IsValid, string? ErrorDetail, string? ModelInfo)> TestKeyAsync(string apiKey, string? model, CancellationToken ct = default);
}
