using FluentAssertions;
using MyAi.Domain.Enums;
using MyAi.Infrastructure.AI.Shared;
using Xunit;

namespace MyAi.Infrastructure.Tests.AI;

public class ExpressionValidatorTests
{
    [Theory]
    [InlineData("NEUTRAL", true)]
    [InlineData("HAPPY", true)]
    [InlineData("SAD", true)]
    [InlineData("SURPRISED", true)]
    [InlineData("ANGRY", true)]
    [InlineData("RELAXED", true)]
    [InlineData("EXCITED", true)]
    [InlineData("CONFUSED", true)]
    [InlineData("THOUGHTFUL", true)]
    [InlineData("CONCERNED", true)]
    [InlineData("FRIENDLY", true)]
    [InlineData("SERIOUS", true)]
    public void IsValid_AllTwelveCanonicalExpressions_ShouldBeValid(string expression, bool expected)
    {
        ExpressionValidator.IsValid(expression).Should().Be(expected);
    }

    [Theory]
    [InlineData("WINK")]
    [InlineData("SMILE")]
    [InlineData("neutral ")]
    [InlineData("")]
    public void IsValid_UnknownOrMalformed_ShouldBeInvalid(string expression)
    {
        ExpressionValidator.IsValid(expression).Should().BeFalse();
    }

    [Fact]
    public void NormalizeOrNeutral_ValidValue_ShouldReturnUppercased()
    {
        ExpressionValidator.NormalizeOrNeutral("happy").Should().Be("HAPPY");
    }

    [Fact]
    public void NormalizeOrNeutral_InvalidValue_ShouldReturnNeutral()
    {
        ExpressionValidator.NormalizeOrNeutral("dancing").Should().Be("NEUTRAL");
    }

    [Fact]
    public void GetAllowedList_ShouldReturnTwelve()
    {
        ExpressionValidator.GetAllowedList().Should().HaveCount(12);
    }
}
