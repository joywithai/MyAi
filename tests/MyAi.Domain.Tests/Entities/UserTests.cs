using FluentAssertions;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.Events;
using MyAi.Domain.Exceptions;
using Xunit;

namespace MyAi.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var user = User.Create("user@example.com", "$2a$12$somehashvalue1234567890", "Rahim");

        user.Email.ToString().Should().Be("user@example.com");
        user.DisplayName.Should().Be("Rahim");
        user.Role.Should().Be(UserRole.PublicUser);
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrow()
    {
        var act = () => User.Create("not-an-email", "$2a$12$somehashvalue1234567890", "Rahim");

        act.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseUserRegisteredEvent()
    {
        var user = User.Create("user@example.com", "$2a$12$somehashvalue1234567890", "Rahim");

        user.DomainEvents.OfType<UserRegisteredEvent>().Should().ContainSingle();
    }

    [Fact]
    public void ChangeRole_ShouldRaiseRoleChangedEvent()
    {
        var user = User.Create("user@example.com", "$2a$12$somehashvalue1234567890", "Rahim");
        user.ClearDomainEvents();

        user.ChangeRole(UserRole.Subscriber);

        user.Role.Should().Be(UserRole.Subscriber);
        user.DomainEvents.OfType<UserRoleChangedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Deactivate_ActiveUser_ShouldChangeStatus()
    {
        var user = User.Create("user@example.com", "$2a$12$somehashvalue1234567890", "Rahim");

        user.Deactivate();

        user.Status.Should().Be(UserStatus.Inactive);
        user.CanLogin().Should().BeFalse();
    }

    [Fact]
    public void Ban_ActiveUser_ShouldPreventLogin()
    {
        var user = User.Create("user@example.com", "$2a$12$somehashvalue1234567890", "Rahim");

        user.Ban();

        user.CanLogin().Should().BeFalse();
    }
}
