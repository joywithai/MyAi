using FluentAssertions;
using Moq;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Auth.Commands;
using MyAi.Domain.Entities;
using Xunit;

namespace MyAi.Application.Tests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);

    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();

    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGenerator = new();

    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private RegisterUserCommandHandler CreateHandler() => new(
        _userRepository.Object,
        _passwordHasher.Object,
        _jwtTokenGenerator.Object,
        _refreshTokenGenerator.Object,
        _unitOfWork.Object,
        _currentUserService.Object);

    [Fact]
    public async Task Handle_WithNewEmail_ShouldCreateUserAndReturnTokens()
    {
        const string email = "newuser@example.com";
        const string password = "SecurePass123!";

        _userRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasher.Setup(h => h.Hash(password)).Returns("$2a$12$hashedvalue");
        _jwtTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<User>()))
            .Returns((User u) => $"access-token-for-{u.Email}");
        _refreshTokenGenerator.Setup(g => g.GenerateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token-value");

        var result = await CreateHandler().Handle(
            new RegisterUserCommand(email, password, "New User", "127.0.0.1"),
            CancellationToken.None);

        result.AccessToken.Should().Be("access-token-for-" + email.ToLowerInvariant());
        result.RefreshToken.Should().Be("refresh-token-value");
        result.ExpiresIn.Should().Be(900);

        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.DisplayName == "New User"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowConflictException()
    {
        var existing = User.Create("taken@example.com", "$2a$12$hash", "Existing");

        _userRepository
            .Setup(r => r.GetByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var act = () => CreateHandler().Handle(
            new RegisterUserCommand("taken@example.com", "SecurePass123!", "New User", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
