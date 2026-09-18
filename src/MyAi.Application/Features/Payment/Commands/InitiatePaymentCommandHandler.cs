using MediatR;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Payment.Strategies;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Payment.Commands;

public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, CheckoutData>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly DemoPaymentStrategy _demoPaymentStrategy;
    private readonly StripePaymentStrategy _stripePaymentStrategy;
    private readonly SslCommerzPaymentStrategy _sslCommerzPaymentStrategy;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly ICurrentUserService _currentUserService;

    public InitiatePaymentCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        DemoPaymentStrategy demoPaymentStrategy,
        StripePaymentStrategy stripePaymentStrategy,
        SslCommerzPaymentStrategy sslCommerzPaymentStrategy,
        ISystemSettingRepository systemSettingRepository,
        ICurrentUserService currentUserService)
    {
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _demoPaymentStrategy = demoPaymentStrategy;
        _stripePaymentStrategy = stripePaymentStrategy;
        _sslCommerzPaymentStrategy = sslCommerzPaymentStrategy;
        _systemSettingRepository = systemSettingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CheckoutData> Handle(InitiatePaymentCommand command, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var plan = await _subscriptionRepository.GetPlanByIdAsync(command.PlanId, ct)
                   ?? throw new NotFoundException("Plan", command.PlanId);

        var activeSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, ct);

        if (activeSubscription is not null && activeSubscription.IsActive())
        {
            throw new ValidationException(
                "You already have an active subscription. Cancel it before subscribing again.");
        }

        var providerName = (await _systemSettingRepository.GetValueAsync("payment_provider", ct) ?? "demo").ToLowerInvariant();
        var providerEnum = providerName switch
        {
            "stripe" => PaymentProvider.Stripe,
            "sslcommerz" => PaymentProvider.SslCommerz,
            _ => PaymentProvider.Demo
        };

        var strategy = SelectPaymentStrategy(providerName);

        var transaction = PaymentTransaction.Create(
            userId, plan.Id, plan.PriceAmount, plan.PriceCurrency, providerEnum);

        await _paymentRepository.AddAsync(transaction, ct);

        var request = new PaymentRequest(
            transaction.Id, userId, plan.Id, plan.PriceAmount, plan.PriceCurrency, null, null);

        var checkout = await strategy.InitiateAsync(request, ct);

        return checkout with { TransactionId = transaction.Id };
    }

    private IPaymentStrategy SelectPaymentStrategy(string provider) => provider switch
    {
        "stripe" => _stripePaymentStrategy,
        "sslcommerz" => _sslCommerzPaymentStrategy,
        _ => _demoPaymentStrategy
    };
}
