namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IFeatureFlagsRepository
{
    Task<Domain.Entities.RoleFeatureFlags?> GetByRoleAsync(Domain.Enums.UserRole role, CancellationToken ct = default);

    Task<List<Domain.Entities.RoleFeatureFlags>> GetAllAsync(CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.RoleFeatureFlags flags, CancellationToken ct = default);
}
