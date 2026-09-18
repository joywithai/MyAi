using FluentAssertions;
using MyAi.Domain.ValueObjects;
using Xunit;

namespace MyAi.Domain.Tests.ValueObjects;

public class ExpressionSegmentTests
{
    [Fact]
    public void Create_ValidExpression_ShouldSucceed()
    {
        var segment = ExpressionSegment.Create("HAPPY", "আমি ভালো আছি");

        segment.Expression.Should().Be("HAPPY");
        segment.Text.Should().Be("আমি ভালো আছি");
    }

    [Fact]
    public void Create_InvalidExpression_ShouldNormalizeToNeutral()
    {
        var segment = ExpressionSegment.Create("NOT_A_REAL_EXPRESSION", "hello");

        segment.Expression.Should().Be("NEUTRAL");
    }

    [Fact]
    public void Create_LowercaseExpression_ShouldUppercase()
    {
        var segment = ExpressionSegment.Create("happy", "hello");

        segment.Expression.Should().Be("HAPPY");
    }

    [Fact]
    public void Create_EmptyText_ShouldThrow()
    {
        var act = () => ExpressionSegment.Create("HAPPY", "   ");

        act.Should().Throw<ArgumentException>();
    }
}
