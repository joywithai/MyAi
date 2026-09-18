using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Infrastructure.TTS.Shared;

/// <summary>Strategy: picks the Edge TTS voice for a language / user preference.</summary>
public class VoiceSelector
{
    public const string DefaultBanglaVoice = "bn-BD-NabanitaNeural";

    public const string DefaultEnglishVoice = "en-US-JennyNeural";

    public string SelectVoice(Language language) => language == Language.Bn
        ? DefaultBanglaVoice
        : DefaultEnglishVoice;

    public string SelectVoiceBySettings(VoiceSettings settings) => settings.VoiceName;

    public string NormalizeLanguage(string language) =>
        string.Equals(language, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "bn";

    public bool IsBangla(Language language) => language == Language.Bn;

    public bool IsEnglish(Language language) => language == Language.En;
}
