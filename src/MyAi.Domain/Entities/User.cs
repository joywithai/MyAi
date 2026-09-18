using MyAi.Domain.Enums;
using MyAi.Domain.Events;
using MyAi.Domain.Events.Base;
using MyAi.Domain.Exceptions;
using MyAi.Domain.Interfaces;
using MyAi.Domain.ValueObjects;

namespace MyAi.Domain.Entities;

public class User : Entity, IAuditableEntity
{
    private User() { } // EF Core

    private User(Email email, string passwordHash, string displayName)
    {
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        Role = UserRole.PublicUser;
        Status = UserStatus.Active;
        EmailVerified = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Email Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string DisplayName { get; private set; }

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    public bool EmailVerified { get; private set; }

    public DateTime? EmailVerifiedAt { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; private set; }

    public static User Create(string email, string passwordHash, string displayName)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 100)
        {
            throw new DomainException("Display name must be 1-100 characters.");
        }

        var user = new User(Email.Create(email), passwordHash, displayName.Trim());
        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email, user.DisplayName));
        return user;
    }

    public void UpdateProfile(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 100)
        {
            throw new DomainException("Display name must be 1-100 characters.");
        }

        DisplayName = displayName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole newRole)
    {
        var oldRole = Role;

        if (oldRole == newRole)
        {
            return;
        }

        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserRoleChangedEvent(Id, oldRole.ToString().ToSnakeCase(), newRole.ToString().ToSnakeCase()));
    }

    public void SetLastLogin(DateTime loginAt)
    {
        LastLoginAt = loginAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkEmailVerified()
    {
        EmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => Status = UserStatus.Inactive;

    public void Ban() => Status = UserStatus.Banned;

    public void Activate() => Status = UserStatus.Active;

    public bool CanLogin() => Status == UserStatus.Active;
}

internal static class EnumNamingExtensions
{
    public static string ToSnakeCase(this Enum value) =>
        System.Text.RegularExpressions.Regex.Replace(
            value.ToString(),
            "([a-z])([A-Z])",
            "$1_$2").ToLowerInvariant();
}
