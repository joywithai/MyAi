using MediatR;
using MyAi.Application.Features.Subscription;

namespace MyAi.Application.Features.Payment.Commands;

public record ProcessPaymentResultDto(
    Guid TransactionId,
    string Status,
    bool IsSuccess,
    string? Message,
    UserSubscriptionDto? Subscription);

public record ProcessPaymentCommand(
    Guid TransactionId,
    string? ProviderTransactionId = null,
    IReadOnlyDictionary<string, string>? ProviderData = null) : IRequest<ProcessPaymentResultDto>;
