namespace MyAi.Application.Common.Interfaces.Repositories;

public interface ISettingsRepository
{
    Task<Domain.Entities.UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task AddAsync(Domain.Entities.UserSettings settings, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.UserSettings settings, CancellationToken ct = default);
}
