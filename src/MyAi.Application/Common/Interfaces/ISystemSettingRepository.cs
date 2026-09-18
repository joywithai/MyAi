namespace MyAi.Application.Common.Interfaces.Repositories;

public interface ISystemSettingRepository
{
    Task<Domain.Entities.SystemSetting?> GetByKeyAsync(string key, CancellationToken ct = default);

    Task<List<Domain.Entities.SystemSetting>> GetAllAsync(CancellationToken ct = default);

    Task AddAsync(Domain.Entities.SystemSetting setting, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.SystemSetting setting, CancellationToken ct = default);

    Task<string?> GetValueAsync(string key, CancellationToken ct = default);
}
