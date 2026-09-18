using Microsoft.Extensions.Logging;
using MyAi.Infrastructure.TTS.Abstractions;
using MyAi.Infrastructure.TTS.Shared;

namespace MyAi.Infrastructure.TTS.EdgeTts;

/// <summary>Adapter pattern: adapts the Edge TTS WebSocket protocol to the domain ITtsProvider.</summary>
public class EdgeTtsAdapter : BaseTtsProvider
{
    private readonly EdgeTtsWebSocketClient _client;

    private readonly ILogger<EdgeTtsAdapter> _logger;

    public EdgeTtsAdapter(EdgeTtsWebSocketClient client, VoiceSelector voiceSelector, ILogger<EdgeTtsAdapter> logger)
        : base(voiceSelector)
    {
        _client = client;
        _logger = logger;
    }

    public override string Name => "edge";

    protected override async Task<EdgeTts.Models.EdgeTtsResponse> SynthesizeInternalAsync(
        EdgeTts.Models.EdgeTtsRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Edge TTS synthesis: voice={Voice}, chars={Chars}", request.Voice, request.Text.Length);
        return await _client.SynthesizeAsync(request, ct);
    }
}
