using MyAi.Application.Common.Ai;
using MyAi.Domain.Enums;

namespace MyAi.Application.Common.Interfaces;

/// <summary>Contract for AI providers (OpenRouter, demo, future providers).</summary>
public interface IAiProvider
{
    string Name { get; }

    Task<AiResult> ProcessAsync(AiRequest request, CancellationToken ct = default);
}
