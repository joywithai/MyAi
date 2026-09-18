using FluentAssertions;
using MyAi.Domain.Exceptions;
using MyAi.Domain.ValueObjects;
using Xunit;

namespace MyAi.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("rahim@gmail.com")]
    [InlineData("USER@Example.COM")]
    public void Create_ValidEmail_ShouldSucceed(string input)
    {
        var email = Email.Create(input);

        email.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@example.com")]
    public void Create_InvalidEmail_ShouldThrow(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@example.com");

        a.Should().Be(b);
    }
}
