using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MyAi.Infrastructure.Identity;

/// <summary>
/// Single source of truth for the RS256 RSA keypair used for JWT signing/validation.
/// Loads from Jwt:PrivateKeyPath (PEM) or Jwt:PrivateKeyBase64 (PKCS#8 DER Base64).
/// When neither is configured (local development only) an ephemeral key is generated —
/// the SAME instance is used for signing and validation, so tokens work per-process.
/// </summary>
public class JwtSigningKeyProvider
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<JwtSigningKeyProvider> _logger;

    private readonly Lazy<RsaSecurityKey> _privateKey;

    public JwtSigningKeyProvider(IConfiguration configuration, ILogger<JwtSigningKeyProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _privateKey = new Lazy<RsaSecurityKey>(LoadKey);
    }

    public RsaSecurityKey GetSigningKey() => _privateKey.Value;

    public IEnumerable<SecurityKey> GetValidationKeys() => new SecurityKey[] { _privateKey.Value };

    public bool IsEphemeral =>
        string.IsNullOrWhiteSpace(_configuration["Jwt:PrivateKeyPath"])
        && string.IsNullOrWhiteSpace(_configuration["Jwt:PrivateKeyBase64"]);

    private RsaSecurityKey LoadKey()
    {
        var rsa = RSA.Create();

        var privateBase64 = _configuration["Jwt:PrivateKeyBase64"];
        var privatePath = _configuration["Jwt:PrivateKeyPath"];

        try
        {
            if (!string.IsNullOrWhiteSpace(privateBase64))
            {
                rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateBase64), out _);
                _logger.LogInformation("JWT RSA private key loaded from base64 configuration.");
            }
            else if (!string.IsNullOrWhiteSpace(privatePath) && File.Exists(privatePath))
            {
                rsa.ImportFromPem(File.ReadAllText(privatePath));
                _logger.LogInformation("JWT RSA private key loaded from {Path}.", privatePath);
            }
            else
            {
                _logger.LogWarning(
                    "No JWT RSA private key configured; generating an EPHEMERAL dev key. " +
                    "Tokens are invalidated on restart. Set Jwt:PrivateKeyPath for production.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load the configured JWT private key; using an ephemeral key.");
        }

        return new RsaSecurityKey(rsa);
    }
}
