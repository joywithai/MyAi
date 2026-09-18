namespace MyAi.Application.Common.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Domain.Entities.PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Domain.Entities.PaymentTransaction transaction, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.PaymentTransaction transaction, CancellationToken ct = default);
}
