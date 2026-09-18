using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Tts;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;
using MyAi.Infrastructure.TTS.Shared;

namespace MyAi.Infrastructure.TTS.Abstractions;

/// <summary>
/// Template Method pattern for TTS providers:
/// validate → select voice → synthesize → build result.
/// </summary>
public abstract class BaseTtsProvider : ITtsProvider
{
    protected readonly VoiceSelector VoiceSelector;

    protected BaseTtsProvider(VoiceSelector voiceSelector)
    {
        VoiceSelector = voiceSelector;
    }

    public abstract string Name { get; }

    public async Task<TtsResult> SynthesizeAsync(TtsRequest request, CancellationToken ct = default)
    {
        ValidateInput(request.Text, request.Language);

        var voiceName = SelectVoice(request.Language, request.VoiceSettings);

        var edgeRequest = new EdgeTts.Models.EdgeTtsRequest
        {
            Text = request.Text,
            Voice = voiceName,
            Rate = ToRateString(request.VoiceSettings.Speed),
            Pitch = ToPitchString(request.VoiceSettings.Pitch)
        };

        var synthesis = await SynthesizeInternalAsync(edgeRequest, ct);
        return BuildTtsResult(synthesis, voiceName);
    }

    protected virtual void ValidateInput(string text, Language language)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("TTS input text must not be empty.");
        }

        if (text.Length > 3000)
        {
            throw new ArgumentException("TTS input text exceeds the 3000 character limit.");
        }
    }

    protected virtual string SelectVoice(Language language, VoiceSettings settings) =>
        string.IsNullOrWhiteSpace(settings.VoiceName)
            ? VoiceSelector.SelectVoice(language)
            : VoiceSelector.SelectVoiceBySettings(settings);

    protected abstract Task<EdgeTts.Models.EdgeTtsResponse> SynthesizeInternalAsync(
        EdgeTts.Models.EdgeTtsRequest request, CancellationToken ct);

    protected virtual TtsResult BuildTtsResult(EdgeTts.Models.EdgeTtsResponse response, string voiceUsed)
    {
        if (!response.IsSuccess)
        {
            throw new ExternalServiceException("tts", response.ErrorMessage ?? "Unknown TTS failure");
        }

        return new TtsResult
        {
            Audio = response.Audio,
            ContentType = AudioConverter.NormalizeContentType(response.ContentType),
            DurationMs = AudioConverter.CalculateDurationMs(response.WordBoundaries),
            WordBoundaries = response.WordBoundaries
                .Select(w => new WordBoundary(
                    w.Word, w.StartTimeMs, w.DurationMs, w.TextOffset, w.WordLength))
                .ToList(),
            VoiceUsed = voiceUsed
        };
    }

    private static string ToRateString(float speed)
    {
        var percent = (int)Math.Round((speed - 1f) * 100f);
        return (percent >= 0 ? "+" : string.Empty) + percent + "%";
    }

    private static string ToPitchString(int pitch)
    {
        return (pitch >= 0 ? "+" : string.Empty) + pitch + "Hz";
    }
}
