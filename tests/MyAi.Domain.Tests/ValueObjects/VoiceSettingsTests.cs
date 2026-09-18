using FluentAssertions;
using MyAi.Domain.Enums;
using MyAi.Domain.Exceptions;
using MyAi.Domain.ValueObjects;
using Xunit;

namespace MyAi.Domain.Tests.ValueObjects;

public class VoiceSettingsTests
{
    [Fact]
    public void CreateDefault_ShouldUseNabanitaForBangla()
    {
        var settings = VoiceSettings.CreateDefault(Language.Bn);

        settings.VoiceName.Should().Be("bn-BD-NabanitaNeural");
    }

    [Fact]
    public void Create_SpeedOutOfRange_ShouldThrow()
    {
        var act = () => new VoiceSettings(Language.Bn, "bn-BD-NabanitaNeural", 2.5f, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_PitchOutOfRange_ShouldThrow()
    {
        var act = () => new VoiceSettings(Language.Bn, "bn-BD-NabanitaNeural", 1.0f, 100);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void WithSpeed_ShouldReturnNewInstance()
    {
        var original = VoiceSettings.CreateDefault(Language.Bn);

        var updated = original.WithSpeed(1.5f);

        updated.Speed.Should().Be(1.5f);
        original.Speed.Should().Be(1.0f); // immutable
    }
}
