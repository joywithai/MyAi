namespace MyAi.Application.Common.Interfaces.Repositories;

public interface ICustomAiConfigRepository
{
    Task<Domain.Entities.UserCustomAiConfig?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task UpsertAsync(Domain.Entities.UserCustomAiConfig config, CancellationToken ct = default);

    Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default);
}
