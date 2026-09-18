using FluentAssertions;
using MyAi.Domain.Enums;
using MyAi.Infrastructure.AI.OpenRouter;
using Xunit;

namespace MyAi.Infrastructure.Tests.AI;

public class OpenRouterResponseParserTests
{
    private readonly OpenRouterResponseParser _parser = new();

    private const string ValidResponse =
        """{"reply":"আমি ভালো আছি","script":[{"expression":"HAPPY","text":"আমি ভালো আছি!"},{"expression":"NEUTRAL","text":"আপনার কেমন অনুভব করছেন?"}]}""";

    [Fact]
    public void Parse_ValidJson_ShouldReturnScriptSegments()
    {
        var result = _parser.Parse(ValidResponse, Language.Bn);

        result.Segments.Should().HaveCount(2);
        result.Segments[0].Expression.Should().Be("HAPPY");
        result.Segments[0].Text.Should().Contain("ভালো আছি");
        result.Segments[1].Expression.Should().Be("NEUTRAL");
    }

    [Fact]
    public void Parse_MarkdownFencedJson_ShouldStillParse()
    {
        var fenced = "```json\n" + ValidResponse + "\n```";

        var result = _parser.Parse(fenced, Language.Bn);

        result.Segments.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_InvalidJson_ShouldFallBackToSingleNeutralSegment()
    {
        var result = _parser.Parse("this is not json at all", Language.Bn);

        result.Segments.Should().ContainSingle();
        result.Segments[0].Expression.Should().Be("NEUTRAL");
        result.Segments[0].Text.Should().Be("this is not json at all");
    }

    [Fact]
    public void Parse_EmptyResponse_ShouldReturnFallbackMessage()
    {
        var result = _parser.Parse("", Language.Bn);

        result.Segments.Should().ContainSingle();
        result.Segments[0].Expression.Should().Be("NEUTRAL");
        result.Segments[0].Text.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Parse_ScriptWithInvalidExpressions_ShouldNormalizeToNeutral()
    {
        var json = """{"reply":"ok","script":[{"expression":"WINK","text":"one"},{"expression":"HAPPY","text":"two"}]}""";

        var result = _parser.Parse(json, Language.En);

        result.Segments[0].Expression.Should().Be("NEUTRAL");
        result.Segments[1].Expression.Should().Be("HAPPY");
    }

    [Fact]
    public void Parse_ScriptMissingText_ShouldSkipEmptySegments()
    {
        var json = """{"reply":"ok","script":[{"expression":"HAPPY","text":""},{"expression":"HAPPY","text":"real"}]}""";

        var result = _parser.Parse(json, Language.En);

        result.Segments.Should().ContainSingle(s => s.Text == "real");
    }

    [Fact]
    public void Parse_ScriptMissing_ShouldUseReplyField()
    {
        var json = """{"reply":"just a plain answer"}""";

        var result = _parser.Parse(json, Language.En);

        result.Segments.Should().ContainSingle(s => s.Text == "just a plain answer");
    }
}
