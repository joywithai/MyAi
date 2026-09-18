namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Domain.Entities.User?> FindByEmailAsync(string email, CancellationToken ct = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    Task AddAsync(Domain.Entities.User user, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.User user, CancellationToken ct = default);

    Task UpdateRoleAsync(Guid userId, Domain.Enums.UserRole role, CancellationToken ct = default);

    Task UpdateStatusAsync(Guid userId, Domain.Enums.UserStatus status, CancellationToken ct = default);

    Task<Domain.Entities.RefreshToken?> FindRefreshTokenAsync(string token, CancellationToken ct = default);

    Task AddRefreshTokenAsync(Domain.Entities.RefreshToken token, CancellationToken ct = default);

    Task<Common.Models.PaginatedList<Domain.Entities.User>> GetPagedAsync(
        Common.Models.PagedRequest request,
        Domain.Enums.UserRole? role,
        Domain.Enums.UserStatus? status,
        CancellationToken ct = default);

    Task<int> CleanupExpiredRefreshTokensAsync(CancellationToken ct = default);
}
