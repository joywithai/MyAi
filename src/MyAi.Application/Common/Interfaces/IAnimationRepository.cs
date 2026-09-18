namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IAnimationRepository
{
    Task<Domain.Entities.Animation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<Domain.Entities.Animation>> GetAccessibleByRoleAsync(Domain.Enums.UserRole role, CancellationToken ct = default);

    Task<List<Domain.Entities.Animation>> GetAllActiveAsync(CancellationToken ct = default);

    Task AddAsync(Domain.Entities.Animation animation, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.Animation animation, CancellationToken ct = default);
}
