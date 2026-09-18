namespace MyAi.Infrastructure.Identity.Configuration;

public class JwtConfiguration
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MyAi";

    public string Audience { get; set; } = "MyAi-Web";

    public int AccessTokenExpiryMinutes { get; set; } = 15;

    public int RefreshTokenExpiryDays { get; set; } = 7;

    /// <summary>RSA private key in PEM format (file path). RS256 signing key.</summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>RSA private key as Base64 PKCS#8 DER (inline alternative to PrivateKeyPath).</summary>
    public string? PrivateKeyBase64 { get; set; }

    /// <summary>32-byte key (Base64) used for AES-256 encryption of user API keys.</summary>
    public string? EncryptionKeyBase64 { get; set; }
}
