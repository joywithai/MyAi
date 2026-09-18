using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MyAi.Infrastructure.AI.OpenRouter.Models;

namespace MyAi.Infrastructure.AI.OpenRouter;

/// <summary>Raw HTTP communication with the OpenRouter API.</summary>
public class OpenRouterHttpClient
{
    private readonly HttpClient _httpClient;

    private readonly ILogger<OpenRouterHttpClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public OpenRouterHttpClient(HttpClient httpClient, ILogger<OpenRouterHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OpenRouterResponse> SendCompletionRequestAsync(
        OpenRouterRequest request, string apiKey, CancellationToken ct = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri());
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

        var response = await _httpClient.SendAsync(ct: ct, request: httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await ReadErrorBodyAsync(response);
            _logger.LogWarning("OpenRouter request failed ({StatusCode}): {Body}", (int)response.StatusCode, errorBody);
            throw new InvalidOperationException($"OpenRouter API returned {(int)response.StatusCode}: {errorBody}");
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(JsonOptions, ct);

        if (payload?.Error is not null)
        {
            throw new InvalidOperationException($"OpenRouter API error: {payload.Error.Message}");
        }

        return payload ?? throw new InvalidOperationException("OpenRouter returned an empty response.");
    }

    /// <summary>Lightweight key validity probe (GET /api/v1/key).</summary>
    public async Task<(bool IsValid, string? ErrorDetail)> TestKeyAsync(string apiKey, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://openrouter.ai/api/v1/key");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var body = await ReadErrorBodyAsync(response);
            return (false, $"HTTP {(int)response.StatusCode}: {Truncate(body, 200)}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public string BuildRequestUri() => "https://openrouter.ai/api/v1/chat/completions";

    public async Task<string> ReadErrorBodyAsync(HttpResponseMessage response)
    {
        try
        {
            return Truncate(await response.Content.ReadAsStringAsync(), 500);
        }
        catch
        {
            return "<unreadable>";
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
