using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class UserCustomAiConfig : Entity, IAuditableEntity
{
    private UserCustomAiConfig() { } // EF Core

    private UserCustomAiConfig(Guid userId, string providerName, string encryptedApiKey, string? preferredModel)
    {
        UserId = userId;
        ProviderName = providerName;
        EncryptedApiKey = encryptedApiKey;
        PreferredModel = preferredModel;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string ProviderName { get; private set; }

    public string EncryptedApiKey { get; private set; }

    public string? PreferredModel { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? LastVerifiedAt { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static UserCustomAiConfig Create(
        Guid userId, string provider, string encryptedApiKey, string? preferredModel)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            throw new ArgumentException("Encrypted API key is required.");
        }

        return new UserCustomAiConfig(userId, string.IsNullOrWhiteSpace(provider) ? "openrouter" : provider, encryptedApiKey, preferredModel);
    }

    public void UpdateKey(string encryptedApiKey)
    {
        EncryptedApiKey = encryptedApiKey;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateModel(string? preferredModel)
    {
        PreferredModel = preferredModel;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkVerified() => LastVerifiedAt = DateTime.UtcNow;

    public void Deactivate() => IsActive = false;
}
