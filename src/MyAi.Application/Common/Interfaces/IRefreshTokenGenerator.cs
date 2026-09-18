using MyAi.Domain.Entities;

namespace MyAi.Application.Common.Interfaces;

public interface IRefreshTokenGenerator
{
    Task<RefreshToken> GenerateAsync(Guid userId, string? ip, CancellationToken ct = default);
}
