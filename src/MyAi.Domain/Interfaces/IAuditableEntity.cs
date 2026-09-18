namespace MyAi.Domain.Interfaces;

/// <summary>Entity that carries automatic audit timestamps.</summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
