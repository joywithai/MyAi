using MyAi.Domain.Enums;
using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class Animation : Entity, IAuditableEntity
{
    private Animation() { } // EF Core

    private Animation(string name, string displayName, string? description, UserRole minRole)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        MinRole = minRole;
        IsActive = true;
        SortOrder = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; }

    public string DisplayName { get; private set; }

    public string? Description { get; private set; }

    public UserRole MinRole { get; private set; }

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static Animation Create(string name, string displayName, UserRole minRole)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Animation name is required.");
        }

        return new Animation(name.Trim().ToLowerInvariant().Replace(' ', '-'), displayName.Trim(), null, minRole);
    }

    public void SetDescription(string? description) => Description = description;

    public void UpdateMinRole(UserRole minRole)
    {
        MinRole = minRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSortOrder(int order) => SortOrder = order;

    public bool IsAccessibleByRole(UserRole role)
    {
        if (role == UserRole.Admin || role == UserRole.Subscriber)
        {
            return true;
        }

        return MinRole == UserRole.PublicUser;
    }
}
