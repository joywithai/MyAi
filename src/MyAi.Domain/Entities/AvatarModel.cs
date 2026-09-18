using MyAi.Domain.Enums;
using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class AvatarModel : Entity, IAuditableEntity
{
    private AvatarModel() { } // EF Core

    private AvatarModel(string name, string description, string fileUrl, string? thumbnailUrl, UserRole minRole)
    {
        Name = name;
        Description = description;
        FileUrl = fileUrl;
        ThumbnailUrl = thumbnailUrl;
        MinRole = minRole;
        IsDefault = false;
        IsActive = true;
        SortOrder = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public string FileUrl { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    public long? FileSizeBytes { get; private set; }

    public UserRole MinRole { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static AvatarModel Create(string name, string fileUrl, string? thumbnailUrl, UserRole minRole)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Avatar model name is required.");
        }

        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException("Avatar model file URL is required.");
        }

        return new AvatarModel(name.Trim(), null, fileUrl, thumbnailUrl, minRole);
    }

    public void SetDescription(string? description) => Description = description;

    public void SetThumbnail(string? thumbnailUrl) => ThumbnailUrl = thumbnailUrl;

    public void SetFileSize(long? bytes) => FileSizeBytes = bytes;

    public void MakeDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unset() => IsDefault = false;

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMinRole(UserRole minRole)
    {
        MinRole = minRole;
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
