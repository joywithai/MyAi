namespace MyAi.Domain.Interfaces;

/// <summary>Entity that supports soft delete (is_deleted / deleted_at).</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
