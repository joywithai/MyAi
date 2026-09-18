using System.Security.Cryptography;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Domain.Entities;
using MyAi.Infrastructure.Identity.Configuration;
using Microsoft.Extensions.Options;

namespace MyAi.Infrastructure.Identity;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private readonly IUserRepository _userRepository;

    private readonly JwtConfiguration _configuration;

    public RefreshTokenGenerator(IUserRepository userRepository, IOptions<JwtConfiguration> configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration.Value;
    }

    public async Task<RefreshToken> GenerateAsync(Guid userId, string? ip, CancellationToken ct = default)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);

        var expiresAt = DateTime.UtcNow.AddDays(_configuration.RefreshTokenExpiryDays);
        var refreshToken = RefreshToken.Create(userId, token, expiresAt, ip);

        await _userRepository.AddRefreshTokenAsync(refreshToken, ct);
        return refreshToken;
    }
}
