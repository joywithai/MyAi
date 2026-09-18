using FluentAssertions;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;
using MyAi.Infrastructure.TTS.Shared;
using Xunit;

namespace MyAi.Infrastructure.Tests.TTS;

public class VoiceSelectorTests
{
    private readonly VoiceSelector _selector = new();

    [Fact]
    public void Select_BanglaWithDefaultSettings_ShouldReturnNabanita()
    {
        var voice = _selector.Select(VoiceSettings.CreateDefault(Language.Bn));

        voice.Should().Be("bn-BD-NabanitaNeural");
    }

    [Fact]
    public void Select_EnglishWithDefaultSettings_ShouldReturnJenny()
    {
        var voice = _selector.Select(VoiceSettings.CreateDefault(Language.En));

        voice.Should().Be("en-US-JennyNeural");
    }

    [Fact]
    public void Select_EmptyVoiceName_ShouldFallBackToLanguageDefault()
    {
        var settings = new VoiceSettings(Language.Bn, "", 1.0f, 0);

        _selector.Select(settings).Should().Be("bn-BD-NabanitaNeural");
    }

    [Fact]
    public void Select_CustomVoiceName_ShouldBeRespected()
    {
        var settings = new VoiceSettings(Language.Bn, "bn-BD-PradeepNeural", 1.0f, 0);

        _selector.Select(settings).Should().Be("bn-BD-PradeepNeural");
    }
}
