namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IAvatarModelRepository
{
    Task<Domain.Entities.AvatarModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<Domain.Entities.AvatarModel>> GetActiveAsync(CancellationToken ct = default);

    Task<List<Domain.Entities.AvatarModel>> GetAllAsync(CancellationToken ct = default);

    Task<Domain.Entities.AvatarModel?> GetDefaultAsync(CancellationToken ct = default);

    Task AddAsync(Domain.Entities.AvatarModel model, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.AvatarModel model, CancellationToken ct = default);

    Task UnsetAllDefaultsAsync(CancellationToken ct = default);
}
