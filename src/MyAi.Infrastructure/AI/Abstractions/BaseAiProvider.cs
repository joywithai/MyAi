using MyAi.Application.Common.Ai;
using MyAi.Application.Common.Interfaces;
using MyAi.Infrastructure.AI.Shared;

namespace MyAi.Infrastructure.AI.Abstractions;

/// <summary>
/// Template Method pattern: defines the AI provider flow
/// (validate → build → send → parse → normalize). Subclasses implement the steps.
/// </summary>
public abstract class BaseAiProvider : IAiProvider
{
    public abstract string Name { get; }

    public async Task<AiResult> ProcessAsync(AiRequest request, CancellationToken ct = default)
    {
        ValidateRequest(request);
        var payload = BuildPrompt(request);
        var responseBody = await SendRequestAsync(payload, ct);
        var result = ParseResponse(responseBody, request);
        return ValidateAndNormalizeResult(result);
    }

    protected abstract void ValidateRequest(AiRequest request);

    protected abstract object BuildPrompt(AiRequest request);

    protected abstract Task<string> SendRequestAsync(object payload, CancellationToken ct);

    protected abstract AiResult ParseResponse(string responseBody, AiRequest request);

    /// <summary>Common post-processing: clean segments, guarantee at least one.</summary>
    protected AiResult ValidateAndNormalizeResult(AiResult result)
    {
        var segments = ExpressionValidator.NormalizeSegments(result.Segments);

        if (segments.Count == 0 && ExpressionValidator.IsSafeText(result.ReplyText))
        {
            segments.Add(new Domain.ValueObjects.ExpressionSegment("NEUTRAL", result.ReplyText.Trim()));
        }

        return new AiResult
        {
            ReplyText = result.ReplyText,
            Script = result.Script,
            Segments = segments,
            ModelUsed = result.ModelUsed,
            TokensUsed = result.TokensUsed,
            Language = result.Language
        };
    }
}
