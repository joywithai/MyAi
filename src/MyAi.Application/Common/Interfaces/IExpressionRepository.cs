namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IExpressionRepository
{
    Task<Domain.Entities.Expression?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<Domain.Entities.Expression>> GetAccessibleByRoleAsync(Domain.Enums.UserRole role, CancellationToken ct = default);

    Task<List<Domain.Entities.Expression>> GetAllActiveAsync(CancellationToken ct = default);

    Task AddAsync(Domain.Entities.Expression expression, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.Expression expression, CancellationToken ct = default);
}
