namespace MyAi.Domain.Interfaces;

/// <summary>Base contract for all persisted entities.</summary>
public interface IEntity
{
    Guid Id { get; }
}
