using MediatR;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Payment.Strategies;
using MyAi.Application.Features.Subscription;
using MyAi.Application.Features.Subscription.Commands;
using MyAi.Domain.Entities;
using MyAi.Domain.Events;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Payment.Commands;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMediator _mediator;
    private readonly DemoPaymentStrategy _demoPaymentStrategy;
    private readonly StripePaymentStrategy _stripePaymentStrategy;
    private readonly SslCommerzPaymentStrategy _sslCommerzPaymentStrategy;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IEventPublisher eventPublisher,
        IAuditLogRepository auditLogRepository,
        IMediator mediator,
        DemoPaymentStrategy demoPaymentStrategy,
        StripePaymentStrategy stripePaymentStrategy,
        SslCommerzPaymentStrategy sslCommerzPaymentStrategy)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _eventPublisher = eventPublisher;
        _auditLogRepository = auditLogRepository;
        _mediator = mediator;
        _demoPaymentStrategy = demoPaymentStrategy;
        _stripePaymentStrategy = stripePaymentStrategy;
        _sslCommerzPaymentStrategy = sslCommerzPaymentStrategy;
    }

    public async Task<ProcessPaymentResultDto> Handle(ProcessPaymentCommand command, CancellationToken ct)
    {
        var transaction = await _paymentRepository.GetByIdAsync(command.TransactionId, ct)
                          ?? throw new NotFoundException("PaymentTransaction", command.TransactionId);

        var strategy = SelectPaymentStrategy(transaction.PaymentProvider);
        var callback = new PaymentCallback(transaction.Id, command.ProviderTransactionId, command.ProviderData);

        var result = await strategy.ProcessAsync(callback, ct);

        if (!result.IsSuccess)
        {
            transaction.MarkFailed(result.ProviderResponse);
            await _paymentRepository.UpdateAsync(transaction, ct);

            return new ProcessPaymentResultDto(
                transaction.Id, "failed", false, result.ErrorMessage ?? "Payment failed.", null);
        }

        transaction.MarkSuccess(
            result.ProviderTransactionId ?? $"txn_{transaction.Id:N}", result.ProviderResponse);
        await _paymentRepository.UpdateAsync(transaction, ct);

        // Creates the subscription and upgrades the user role to subscriber.
        var subscription = await _mediator.Send(
            new CreateSubscriptionCommand(transaction.UserId, transaction.PlanId, transaction.Id), ct);

        await _eventPublisher.PublishAsync(
            new PaymentCompletedEvent(
                transaction.Id, transaction.UserId, transaction.PlanId, transaction.Amount,
                transaction.PaymentProvider.ToString().ToLowerInvariant()), ct);

        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                transaction.UserId,
                "payment.completed",
                "payment_transaction",
                transaction.Id,
                null,
                new { transaction.Amount, transaction.Currency, Provider = transaction.PaymentProvider.ToString() }),
            ct);

        return new ProcessPaymentResultDto(
            transaction.Id, "success", true, "Payment completed. Subscription activated.", subscription);
    }

    private IPaymentStrategy SelectPaymentStrategy(PaymentProvider provider) => provider switch
    {
        PaymentProvider.Stripe => _stripePaymentStrategy,
        PaymentProvider.SslCommerz => _sslCommerzPaymentStrategy,
        _ => _demoPaymentStrategy
    };
}
