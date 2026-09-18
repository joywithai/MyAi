using FluentAssertions;
using MyAi.Domain.Entities;
using MyAi.Domain.ValueObjects;
using Xunit;

namespace MyAi.Domain.Tests.Entities;

public class UserSettingsTests
{
    [Fact]
    public void CreateDefault_ShouldHaveDefaultValues()
    {
        var userId = Guid.NewGuid();

        var settings = UserSettings.CreateDefault(userId);

        settings.UserId.Should().Be(userId);
        settings.PreferredLanguage.Should().Be(Language.Bn);
        settings.VoiceName.Should().Be("bn-BD-NabanitaNeural");
        settings.VoiceSpeed.Should().Be(1.00f);
        settings.VoicePitch.Should().Be(0);
        settings.DefaultExpression.Should().Be("FRIENDLY");
        settings.ThemePreference.Should().Be("dark");
        settings.EnabledAnimations.Should().Contain("breathing");
        settings.BlinkEnabled.Should().BeTrue();
        settings.ThinkingPoseEnabled.Should().BeTrue();
    }

    [Fact]
    public void UpdateVoiceSettings_ShouldRaiseSettingsChangedEvent()
    {
        var settings = UserSettings.CreateDefault(Guid.NewGuid());
        settings.ClearDomainEvents();

        settings.UpdateVoiceSettings(new VoiceSettings(Language.En, "en-US-JennyNeural", 1.25f, -5));

        settings.VoiceName.Should().Be("en-US-JennyNeural");
        settings.VoiceSpeed.Should().Be(1.25f);
        settings.VoicePitch.Should().Be(-5);
        settings.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateEnabledAnimations_EmptyList_ShouldFallBackToBreathing()
    {
        var settings = UserSettings.CreateDefault(Guid.NewGuid());

        settings.UpdateEnabledAnimations(new List<string>());

        settings.EnabledAnimations.Should().ContainSingle("breathing");
    }
}
