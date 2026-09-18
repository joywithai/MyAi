namespace MyAi.Domain.Entities;
using System.Text.Json;

public class AuditLog : Entity
{
    private AuditLog() { } // EF Core

    private AuditLog(
        Guid? userId,
        string action,
        string? entityType,
        Guid? entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress,
        string? userAgent)
    {
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid? UserId { get; private set; }

    public string Action { get; private set; }

    public string? EntityType { get; private set; }

    public Guid? EntityId { get; private set; }

    public string? OldValues { get; private set; }

    public string? NewValues { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static AuditLog Create(
        Guid? userId,
        string action,
        string? entityType = null,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        static string? Serialize(object? value) =>
            value is null ? null : System.Text.Json.JsonSerializer.Serialize(value, PaymentTransaction.SegmentJsonOptions);

        return new AuditLog(
            userId, action, entityType, entityId, Serialize(oldValues), Serialize(newValues), ipAddress, userAgent);
    }
}
