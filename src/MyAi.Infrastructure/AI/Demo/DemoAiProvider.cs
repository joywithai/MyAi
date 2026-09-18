using MyAi.Application.Common.Ai;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;
using MyAi.Infrastructure.AI.Abstractions;
using MyAi.Infrastructure.AI.Shared;

namespace MyAi.Infrastructure.AI.Demo;

/// <summary>
/// Demo AI provider: used when no OpenRouter key is configured so the whole
/// avatar pipeline stays demonstrable. Produces a friendly canned reply with
/// valid expression segments — clearly labelled as demo mode.
/// </summary>
public class DemoAiProvider : BaseAiProvider
{
    public override string Name => "demo";

    protected override void ValidateRequest(AiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new InvalidOperationException("AI request message must not be empty.");
        }
    }

    protected override object BuildPrompt(AiRequest request) => request;

    protected override Task<string> SendRequestAsync(object payload, CancellationToken ct) =>
        Task.FromResult(string.Empty);

    protected override AiResult ParseResponse(string responseBody, AiRequest request)
    {
        var isBangla = request.Language == Language.Bn;

        var segments = new List<ExpressionSegment>
        {
            new("HAPPY",
                isBangla
                    ? "আসসালামু আলাইকুম! আমি MyAi — আপনার ভার্চুয়াল বন্ধু।"
                    : "Hello! I am MyAi — your virtual companion."),
            new("FRIENDLY",
                isBangla
                    ? "এই মুহূর্তে আমি ডেমো মোডে আছি, তাই আমার উত্তরগুলো সীমিত।"
                    : "I'm running in demo mode right now, so my answers are limited."),
            new("THOUGHTFUL",
                isBangla
                    ? "সাবস্ক্রাইবার হলে আপনার নিজের AI API key যোগ করে আমাকে পুরো ক্ষমতায় চালাতে পারবেন!"
                    : "Subscribe and add your own AI API key to unlock my full potential!")
        };

        segments.RemoveAll(s => !ExpressionValidator.IsSafeText(s.Text));

        var reply = string.Join(" ", segments.Select(s => s.Text));

        return new AiResult
        {
            ReplyText = reply,
            Script = null,
            Segments = segments,
            ModelUsed = "demo",
            TokensUsed = 0,
            Language = isBangla ? "bn" : "en"
        };
    }
}
