using MyAi.Domain.Entities;

namespace MyAi.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Generates an RS256-signed JWT access token for the user.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Access token lifetime in seconds (default 15 minutes).</summary>
    int GetAccessTokenExpirySeconds();
}
