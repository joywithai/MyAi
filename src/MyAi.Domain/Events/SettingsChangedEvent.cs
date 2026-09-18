namespace MyAi.Domain.Events;

public class SettingsChangedEvent : DomainEvent
{
    public SettingsChangedEvent(Guid userId, IReadOnlyList<string> changedFields)
    {
        UserId = userId;
        ChangedFields = changedFields;
    }

    public Guid UserId { get; }
    public IReadOnlyList<string> ChangedFields { get; }
}
