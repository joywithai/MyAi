using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using MyAi.Infrastructure.TTS.EdgeTts.Models;

namespace MyAi.Infrastructure.TTS.EdgeTts;

/// <summary>
/// Low-level Edge TTS WebSocket protocol client.
/// Flow: connect → speech.config → ssml → collect audio + word boundaries → turn.end.
/// </summary>
public class EdgeTtsWebSocketClient
{
    private readonly EdgeTtsMessageBuilder _messageBuilder;

    private readonly ILogger<EdgeTtsWebSocketClient> _logger;

    public EdgeTtsWebSocketClient(EdgeTtsMessageBuilder messageBuilder, ILogger<EdgeTtsWebSocketClient> logger)
    {
        _messageBuilder = messageBuilder;
        _logger = logger;
    }

    public async Task<EdgeTtsResponse> SynthesizeAsync(EdgeTtsRequest request, CancellationToken ct = default)
    {
        var requestId = Guid.NewGuid().ToString("N").Replace("-", string.Empty)[..32];
        var audioBuffer = new MemoryStream();
        var wordBoundaries = new List<WordBoundaryData>();
        ClientWebSocket? socket = null;

        try
        {
            socket = await ConnectAsync(requestId, ct);
            await SendSpeechConfigAsync(socket, requestId, request.OutputFormat, ct);
            await SendSsmlAsync(socket, requestId, request.Text, request.Voice, request.Rate, request.Pitch, ct);

            var completed = false;

            while (!completed && !ct.IsCancellationRequested)
            {
                var (data, isText) = await ReceiveMessageAsync(socket, ct);

                if (data.Length == 0)
                {
                    break;
                }

                if (isText)
                {
                    var message = Encoding.UTF8.GetString(data);

                    if (message.Contains("Path:turn.end", StringComparison.Ordinal))
                    {
                        completed = true;
                    }
                    else if (message.Contains("Path:audio.metadata", StringComparison.Ordinal))
                    {
                        ParseWordBoundary(message, wordBoundaries);
                    }
                    else if (message.Contains("Path:response.error", StringComparison.Ordinal))
                    {
                        return new EdgeTtsResponse { ErrorMessage = "Edge TTS returned response.error" };
                    }
                }
                else
                {
                    ParseAudioChunk(data, audioBuffer);
                }
            }

            return new EdgeTtsResponse
            {
                Audio = audioBuffer.ToArray(),
                WordBoundaries = wordBoundaries,
                ContentType = "audio/mpeg"
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Edge TTS synthesis failed: {Message}", ex.Message);
            return new EdgeTtsResponse { ErrorMessage = ex.Message };
        }
        finally
        {
            await CloseSocketSafelyAsync(socket);
        }
    }

    public async Task<ClientWebSocket> ConnectAsync(string requestId, CancellationToken ct)
    {
        var socket = new ClientWebSocket();
        socket.Options.SetRequestHeader("Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold");
        socket.Options.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
        socket.Options.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0");

        await socket.ConnectAsync(new Uri(EdgeTtsMessageBuilder.WssUrl), ct);
        return socket;
    }

    public async Task SendSpeechConfigAsync(
        ClientWebSocket socket, string requestId, string outputFormat, CancellationToken ct)
    {
        var config = _messageBuilder.BuildSpeechConfigMessage(requestId, outputFormat);
        await socket.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(config)),
            WebSocketMessageType.Text,
            endOfMessage: true,
            ct);
    }

    public async Task SendSsmlAsync(
        ClientWebSocket socket,
        string requestId,
        string text,
        string voice,
        string rate = "+0%",
        string pitch = "+0Hz",
        CancellationToken ct = default)
    {
        var ssml = _messageBuilder.BuildSsmlMessage(requestId, text, voice);
        await socket.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(ssml)),
            WebSocketMessageType.Text,
            endOfMessage: true,
            ct);
    }

    private static async Task<(byte[] Data, bool IsText)> ReceiveMessageAsync(ClientWebSocket socket, CancellationToken ct)
    {
        using var buffer = new MemoryStream();

        WebSocketReceiveResult result;

        do
        {
            var chunk = new byte[16 * 1024];
            result = await socket.ReceiveAsync(new ArraySegment<byte>(chunk), ct);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return (Array.Empty<byte>(), false);
            }

            buffer.Write(chunk, 0, result.Count);
        }
        while (!result.EndOfMessage);

        return (buffer.ToArray(), result.MessageType == WebSocketMessageType.Text);
    }

    /// <summary>Binary frames: 2-byte big-endian header length + header + payload audio.</summary>
    public void ParseAudioChunk(byte[] data, MemoryStream audioBuffer)
    {
        if (data.Length < 2)
        {
            return;
        }

        var headerLength = (data[0] << 8) | data[1];

        if (data.Length <= headerLength + 2)
        {
            return;
        }

        audioBuffer.Write(data, headerLength + 2, data.Length - headerLength - 2);
    }

    /// <summary>Extracts word timing from audio.metadata JSON payloads.</summary>
    public void ParseWordBoundary(string message, List<WordBoundaryData> boundaries)
    {
        try
        {
            var metadataStart = message.IndexOf('{');

            if (metadataStart < 0)
            {
                return;
            }

            var json = message[metadataStart..];

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Metadata", out var metadata) ||
                !metadata.TryGetProperty("MetadataJson", out var metadataJson))
            {
                return;
            }

            var inner = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(metadataJson.GetString() ?? "{}");

            if (!inner.TryGetProperty("Data", out var dataArray))
            {
                return;
            }

            foreach (var item in dataArray.EnumerateArray())
            {
                if (!item.TryGetProperty("Type", out var typeEl) ||
                    !string.Equals(typeEl.GetString(), "WordBoundary", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!item.TryGetProperty("Data", out var dataEl))
                {
                    continue;
                }

                var text = dataEl.TryGetProperty("Text", out var textEl) ? textEl.GetString() ?? string.Empty : string.Empty;
                var offsetTicks = dataEl.TryGetProperty("Offset", out var offsetEl) ? offsetEl.GetInt64() : 0;
                var durationTicks = dataEl.TryGetProperty("Duration", out var durationEl) ? durationEl.GetInt64() : 0;
                var textOffset = dataEl.TryGetProperty("TextOffset", out var textOffsetEl) ? textOffsetEl.GetInt32() : 0;
                var wordLength = dataEl.TryGetProperty("WordLength", out var wordLengthEl) ? wordLengthEl.GetInt32() : text.Length;

                boundaries.Add(new WordBoundaryData
                {
                    Word = text,
                    StartTimeMs = ConvertTicksToMilliseconds(offsetTicks),
                    DurationMs = ConvertTicksToMilliseconds(durationTicks),
                    TextOffset = textOffset,
                    WordLength = wordLength
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse Edge TTS word boundary metadata");
        }
    }

    /// <summary>Edge TTS timestamps are in 100-nanosecond ticks.</summary>
    public static int ConvertTicksToMilliseconds(long ticks) => (int)(ticks / 10_000);

    public async Task CloseSocketSafelyAsync(ClientWebSocket? socket)
    {
        if (socket is null)
        {
            return;
        }

        try
        {
            if (socket.State == WebSocketState.Open)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", cts.Token);
            }
        }
        catch
        {
            // Swallow close-time failures; the socket is being disposed anyway.
        }
        finally
        {
            socket.Dispose();
        }
    }
}
