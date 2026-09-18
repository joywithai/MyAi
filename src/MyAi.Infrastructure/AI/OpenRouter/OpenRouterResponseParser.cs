using System.Text.Json;
using MyAi.Application.Common.Ai;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;
using MyAi.Infrastructure.AI.Shared;
using OpenRouterResponseModel = MyAi.Infrastructure.AI.OpenRouter.Models.OpenRouterResponse;

namespace MyAi.Infrastructure.AI.OpenRouter;

/// <summary>Parses raw OpenRouter responses into normalized AiResults.</summary>
public class OpenRouterResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AiResult ParseResponse(OpenRouterResponseModel response, Language language)
    {
        var rawContent = ExtractContent(response);

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return CreateFallbackResult("(empty response)", language);
        }

        var (reply, segments) = ParseSegments(rawContent, language);

        return new AiResult
        {
            ReplyText = string.IsNullOrWhiteSpace(reply) ? rawContent.Trim() : reply.Trim(),
            Script = rawContent,
            Segments = segments,
            ModelUsed = response.Model,
            TokensUsed = response.Usage?.TotalTokens,
            Language = language == Language.Bn ? "bn" : "en"
        };
    }

    public string ExtractContent(OpenRouterResponseModel response) =>
        response.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;

    /// <summary>
    /// Accepts either a JSON object {"reply": ..., "script": [...]} or a raw text reply.
    /// Code fences are removed before parsing.
    /// </summary>
    public (string? Reply, List<ExpressionSegment> Segments) ParseSegments(string content, Language language)
    {
        var cleaned = RemoveCodeFences(content);

        try
        {
            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            string? reply = null;
            var segments = new List<ExpressionSegment>();

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("reply", out var replyEl) && replyEl.ValueKind == JsonValueKind.String)
                {
                    reply = replyEl.GetString();
                }

                if (root.TryGetProperty("script", out var scriptEl) && scriptEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in scriptEl.EnumerateArray())
                    {
                        var expression = item.TryGetProperty("expression", out var exprEl)
                            ? exprEl.GetString()
                            : null;
                        var text = item.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;

                        if (ExpressionValidator.IsSafeText(text))
                        {
                            segments.Add(ExpressionSegment.Create(
                                ExpressionValidator.Normalize(expression), text!.Trim()));
                        }
                    }
                }
            }

            if (segments.Count == 0)
            {
                // Fallback: treat the whole content as a single neutral segment.
                segments.Add(new ExpressionSegment("NEUTRAL", cleaned.Trim()));
            }

            return (reply, segments);
        }
        catch (JsonException)
        {
            return (null, new List<ExpressionSegment> { new("NEUTRAL", cleaned.Trim()) });
        }
    }

    public AiResult CreateFallbackResult(string rawContent, Language language) =>
        AiResult.Fallback(rawContent, language == Language.Bn ? "bn" : "en");

    public string NormalizeExpression(string? expression) => ExpressionValidator.Normalize(expression);

    private static string RemoveCodeFences(string content)
    {
        var trimmed = content.Trim();

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = trimmed.IndexOf('\n');

            if (firstNewline >= 0)
            {
                trimmed = trimmed[(firstNewline + 1)..];
            }

            var closingFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);

            if (closingFence >= 0)
            {
                trimmed = trimmed[..closingFence];
            }
        }

        return trimmed.Trim();
    }
}
