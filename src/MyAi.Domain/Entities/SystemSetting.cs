namespace MyAi.Domain.Entities;

/// <summary>System-wide key-value setting (key is the primary key).</summary>
public class SystemSetting : Entity
{
    private SystemSetting() { } // EF Core

    private SystemSetting(string key, string value, string? description)
    {
        Id = Guid.NewGuid();
        Key = key;
        Value = value;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public new string Key { get; private set; }

    public string Value { get; private set; }

    public string? Description { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public DateTime UpdatedAt { get; set; }

    public static SystemSetting Create(string key, string value, string? description) =>
        new(key, value, description);

    public void UpdateValue(string value, Guid? updatedBy)
    {
        Value = value;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
