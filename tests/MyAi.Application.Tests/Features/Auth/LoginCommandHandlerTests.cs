using FluentAssertions;
using Moq;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Auth.Commands;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;
using Xunit;

namespace MyAi.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private static User CreateActiveUser(string passwordHash) =>
        User.Create("user@example.com", passwordHash, "Test User");

    private static Mock<IUserRepository> UserRepositoryMock(User? user)
    {
        var mock = new Mock<IUserRepository>(MockBehavior.Strict);
        mock.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return mock;
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        var user = CreateActiveUser("$2a$12$correcthash");
        user.MarkEmailVerified();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Verify("$2a$12$correcthash", "Pass123!")).Returns(true);

        var jwt = new Mock<IJwtTokenGenerator>();
        jwt.Setup(g => g.GenerateToken(user)).Returns("access-token");

        var refresh = new Mock<IRefreshTokenGenerator>();
        refresh.Setup(g => g.GenerateAsync(user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token");

        var handler = new LoginCommandHandler(
            UserRepositoryMock(user).Object,
            passwordHasher.Object,
            jwt.Object,
            refresh.Object,
            new Mock<IUnitOfWork>().Object);

        var result = await handler.Handle(
            new LoginCommand("user@example.com", "Pass123!", "127.0.0.1"),
            CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldThrowUnauthorizedException()
    {
        var user = CreateActiveUser("$2a$12$correcthash");

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var handler = new LoginCommandHandler(
            UserRepositoryMock(user).Object,
            passwordHasher.Object,
            new Mock<IJwtTokenGenerator>().Object,
            new Mock<IRefreshTokenGenerator>().Object,
            new Mock<IUnitOfWork>().Object);

        var act = () => handler.Handle(
            new LoginCommand("user@example.com", "WrongPass!", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ShouldThrowUnauthorizedException()
    {
        var passwordHasher = new Mock<IPasswordHasher>();

        var handler = new LoginCommandHandler(
            UserRepositoryMock(null).Object,
            passwordHasher.Object,
            new Mock<IJwtTokenGenerator>().Object,
            new Mock<IRefreshTokenGenerator>().Object,
            new Mock<IUnitOfWork>().Object);

        var act = () => handler.Handle(
            new LoginCommand("ghost@example.com", "Whatever1!", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WithBannedUser_ShouldThrowForbiddenException()
    {
        var user = CreateActiveUser("$2a$12$correcthash");
        user.Ban();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var handler = new LoginCommandHandler(
            UserRepositoryMock(user).Object,
            passwordHasher.Object,
            new Mock<IJwtTokenGenerator>().Object,
            new Mock<IRefreshTokenGenerator>().Object,
            new Mock<IUnitOfWork>().Object);

        var act = () => handler.Handle(
            new LoginCommand("user@example.com", "Pass123!", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
