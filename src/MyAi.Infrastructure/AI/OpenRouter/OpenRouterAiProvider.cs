using Microsoft.Extensions.Logging;
using System.Text.Json;
using MyAi.Application.Common.Ai;
using MyAi.Application.Common.Interfaces;
using MyAi.Domain.Enums;
using MyAi.Infrastructure.AI.Abstractions;
using MyAi.Infrastructure.AI.OpenRouter.Models;
using MyAi.Infrastructure.AI.Shared;

namespace MyAi.Infrastructure.AI.OpenRouter;

public class OpenRouterAiProvider : BaseAiProvider
{
    private readonly OpenRouterHttpClient _httpClient;

    private readonly OpenRouterPromptBuilder _promptBuilder;

    private readonly OpenRouterResponseParser _parser;

    private readonly string? _apiKey;

    private readonly string _defaultModel;

    public OpenRouterAiProvider(
        OpenRouterHttpClient httpClient,
        OpenRouterPromptBuilder promptBuilder,
        OpenRouterResponseParser parser,
        string? apiKey,
        string defaultModel)
    {
        _httpClient = httpClient;
        _promptBuilder = promptBuilder;
        _parser = parser;
        _apiKey = apiKey;
        _defaultModel = string.IsNullOrWhiteSpace(defaultModel)
            ? "google/gemini-2.0-flash-001:free"
            : defaultModel;
    }

    public override string Name => "openrouter";

    protected override void ValidateRequest(AiRequest request)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "No OpenRouter API key configured. Set OpenRouter:ApiKey or add a custom key.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException("AI request message must not be empty.");
        }
    }

    protected override object BuildPrompt(AiRequest request)
    {
        var language = NormalizeLanguage(request.Language.ToString()) == "en" ? Language.En : Language.Bn;
        var openRouterRequest = new OpenRouterRequest
        {
            Model = string.IsNullOrWhiteSpace(request.PreferredModel) ? _defaultModel : request.PreferredModel!,
            Messages =
            {
                new OpenRouterMessage
                {
                    Role = "system",
                    Content = _promptBuilder.BuildSystemPrompt(language, request.AccessibleExpressions)
                }
            }
        };

        foreach (var historyItem in request.History.TakeLast(10))
        {
            openRouterRequest.Messages.Add(new OpenRouterMessage
            {
                Role = historyItem.Role == "user" ? "user" : "assistant",
                Content = historyItem.Content
            });
        }

        openRouterRequest.Messages.Add(new OpenRouterMessage
        {
            Role = "user",
            Content = _promptBuilder.BuildUserPrompt(request.Message, language)
        });

        return openRouterRequest;
    }

    protected override async Task<string> SendRequestAsync(object payload, CancellationToken ct)
    {
        var request = (OpenRouterRequest)payload;
        var response = await _httpClient.SendCompletionRequestAsync(request, _apiKey!, ct);
        return System.Text.Json.JsonSerializer.Serialize(response);
    }

    protected override AiResult ParseResponse(string responseBody, AiRequest request)
    {
        var response = System.Text.Json.JsonSerializer.Deserialize<OpenRouterResponse>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        if (response is null)
        {
            return _parser.CreateFallbackResult("(unparseable response)", request.Language);
        }

        return _parser.ParseResponse(response, request.Language);
    }

    public string NormalizeLanguage(string language) =>
        language.Equals("En", StringComparison.OrdinalIgnoreCase)
            || language.Equals("en", StringComparison.OrdinalIgnoreCase)
            ? "en"
            : "bn";
}
