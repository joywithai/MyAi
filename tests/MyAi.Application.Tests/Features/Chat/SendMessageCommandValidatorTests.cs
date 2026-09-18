using FluentAssertions;
using MyAi.Application.Features.Chat.Commands;
using Xunit;

namespace MyAi.Application.Tests.Features.Chat;

public class SendMessageCommandValidatorTests
{
    private readonly SendMessageCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyMessage_ShouldFail()
    {
        var result = _validator.Validate(new SendMessageCommand(Guid.NewGuid(), "  ", "bn"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_MessageOver500Chars_ShouldFail()
    {
        var result = _validator.Validate(new SendMessageCommand(Guid.NewGuid(), new string('a', 501), "bn"));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("bn-US")]
    [InlineData("")]
    public void Validate_InvalidLanguage_ShouldFail(string language)
    {
        var result = _validator.Validate(new SendMessageCommand(Guid.NewGuid(), "hello", language));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ValidBanglaMessage_ShouldPass()
    {
        var result = _validator.Validate(new SendMessageCommand(Guid.NewGuid(), "আপনি কেমন আছেন?", "bn"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EnglishWithoutConversationId_ShouldPass()
    {
        var result = _validator.Validate(new SendMessageCommand(null, "Hello there", "en"));

        result.IsValid.Should().BeTrue();
    }
}
