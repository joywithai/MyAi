using MyAi.Application.Common.Tts;

namespace MyAi.Application.Common.Interfaces;

/// <summary>Contract for text-to-speech providers.</summary>
public interface ITtsProvider
{
    string Name { get; }

    Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default);
}
