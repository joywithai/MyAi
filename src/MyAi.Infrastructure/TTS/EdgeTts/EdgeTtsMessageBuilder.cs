using System.Text;
using System.Xml;

namespace MyAi.Infrastructure.TTS.EdgeTts;

/// <summary>
/// Builds the Edge TTS WebSocket protocol messages:
/// speech.config → SSML request → end-of-stream sentinel.
/// </summary>
public class EdgeTtsMessageBuilder
{
    public const string WssUrl =
        "wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken=6A5AA1D4EAFF4E9FB37E23D68491D6F4";

    public string BuildSpeechConfigMessage(string requestId, string outputFormat) =>
        "X-Timestamp:" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK") + "\r\n" +
        "Content-Type:application/json; charset=utf-8\r\n" +
        "Path:speech.config\r\n\r\n" +
        "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"true\"},\"outputFormat\":\"" +
        outputFormat + "\"}}}}";

    public string BuildSsmlMessage(string requestId, string text, string voice) =>
        "X-RequestId:" + requestId + "\r\n" +
        "Content-Type:application/ssml+xml\r\n" +
        "X-Timestamp:" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK") + "Z\r\n" +
        "Path:ssml\r\n\r\n" +
        BuildSsml(text, voice);

    public string BuildEndOfStreamMessage(string requestId) =>
        "X-RequestId:" + requestId + "\r\n" +
        "Content-Type:application/json; charset=utf-8\r\n" +
        "Path:turn.end\r\n\r\n";

    private static string BuildSsml(string text, string voice)
    {
        var escaped = EscapeXml(text);

        return "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>" +
               $"<voice name='{System.Security.SecurityElement.Escape(voice)}'>" +
               $"<prosody rate='+0%' pitch='+0Hz'>{escaped}</prosody>" +
               "</voice></speak>";
    }

    /// <summary>XML-escapes arbitrary text for safe SSML embedding.</summary>
    public static string EscapeXml(string text) => SecurityElementEscape(text);

    private static string SecurityElementEscape(string text)
    {
        var sb = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            sb.Append(ch switch
            {
                '<' => "&lt;",
                '>' => "&gt;",
                '&' => "&amp;",
                '"' => "&quot;",
                '\'' => "&apos;",
                _ => ch
            });
        }

        return sb.ToString();
    }
}
