using FluentAssertions;
using MyAi.Domain.ValueObjects;
using MyAi.Infrastructure.TTS.EdgeTts;
using Xunit;

namespace MyAi.Infrastructure.Tests.TTS;

public class EdgeTtsMessageBuilderTests
{
    private readonly EdgeTtsMessageBuilder _builder = new();

    [Fact]
    public void BuildSsml_ShouldContainVoiceAndText()
    {
        var ssml = _builder.BuildSsml("bn-BD-NabanitaNeural", "আপনি কেমন আছেন?", 1.0f, 0);

        ssml.Should().Contain("bn-BD-NabanitaNeural");
        ssml.Should().Contain("আপনি কেমন আছেন?");
        ssml.Should().StartWith("<speak");
    }

    [Fact]
    public void BuildSsml_SpeedFaster_ShouldEncodePositiveRate()
    {
        var ssml = _builder.BuildSsml("en-US-JennyNeural", "hello", 1.5f, 0);

        ssml.Should().Contain("rate=\"+50%\"");
    }

    [Fact]
    public void BuildSsml_SpeedSlower_ShouldEncodeNegativeRate()
    {
        var ssml = _builder.BuildSsml("en-US-JennyNeural", "hello", 0.75f, 0);

        ssml.Should().Contain("rate=\"-25%\"");
    }

    [Fact]
    public void BuildSsml_PositivePitch_ShouldEncodePlusHz()
    {
        var ssml = _builder.BuildSsml("en-US-JennyNeural", "hello", 1.0f, 20);

        ssml.Should().Contain("pitch=\"+20Hz\"");
    }

    [Fact]
    public void BuildSsml_XmlSpecialCharacters_ShouldBeEscaped()
    {
        var ssml = _builder.BuildSsml("en-US-JennyNeural", "a < b & c > d", 1.0f, 0);

        ssml.Should().NotContain("a < b & c > d");
        ssml.Should().Contain("&lt;").And.Contain("&amp;");
    }

    [Fact]
    public void BuildSpeechConfigMessage_ShouldUseExpectedFormat()
    {
        var message = _builder.BuildSpeechConfigMessage("req-1");

        message.Should().Be("X-Timestamp:req-1\r\nContent-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n");
    }

    [Fact]
    public void BuildSsmlMessage_ShouldEndWithPathSsml()
    {
        var message = _builder.BuildSsmlMessage("req-1", "<speak/>", "req-1-guid");

        message.Should().EndWith("Path:ssml\r\n\r\n<speak/>");
        message.Should().Contain("X-RequestId:req-1-guid");
    }
}
