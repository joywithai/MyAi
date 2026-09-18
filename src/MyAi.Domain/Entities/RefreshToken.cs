namespace MyAi.Domain.Entities;

public class RefreshToken : Entity
{
    private RefreshToken() { } // EF Core

    private RefreshToken(Guid userId, string token, DateTime expiresAt, string? createdByIp)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
        CreatedByIp = createdByIp;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string Token { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? ReplacedByToken { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt, string? ip) =>
        new(userId, token, expiresAt, ip);

    public void Revoke(string? replacedByToken)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }

    public bool IsActive(DateTime utcNow) => !IsRevoked && ExpiresAt > utcNow;
}
