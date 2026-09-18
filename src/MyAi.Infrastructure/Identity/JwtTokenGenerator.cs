using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Identity;

/// <summary>Generates RS256-signed JWT access tokens with role claims.</summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSigningKeyProvider _signingKeyProvider;

    private readonly string _issuer;

    private readonly string _audience;

    private readonly int _expiryMinutes;

    public JwtTokenGenerator(
        JwtSigningKeyProvider signingKeyProvider,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _signingKeyProvider = signingKeyProvider;
        _issuer = configuration["Jwt:Issuer"] ?? "MyAi";
        _audience = configuration["Jwt:Audience"] ?? "MyAi-Web";
        _expiryMinutes = int.TryParse(configuration["Jwt:AccessTokenExpiryMinutes"], out var minutes)
            ? minutes
            : 15;
    }

    public string GenerateAccessToken(User user)
    {
        var credentials = new SigningCredentials(_signingKeyProvider.GetSigningKey(), SecurityAlgorithms.RsaSha256);
        var expiry = DateTime.UtcNow.AddMinutes(_expiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Expires = expiry,
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(GenerateTokenClaims(user))
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public IList<Claim> GenerateTokenClaims(User user) => new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new("name", user.DisplayName),
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email.ToString()),
        new(ClaimTypes.Role, RoleConstants.ToRoleString(user.Role))
    };

    public int GetAccessTokenExpirySeconds() => _expiryMinutes * 60;
}
