using MyAi.Application.Common.Models;

namespace MyAi.Application.Features.Payment.Strategies;

/// <summary>Payment session initiation request.</summary>
public record PaymentRequest(
    Guid TransactionId,
    Guid UserId,
    Guid PlanId,
    decimal Amount,
    string Currency,
    string? ReturnUrl,
    string? CancelUrl);

/// <summary>Payment provider callback data.</summary>
public record PaymentCallback(
    Guid TransactionId,
    string? ProviderTransactionId,
    IReadOnlyDictionary<string, string>? ProviderData);

/// <summary>Verified payment result.</summary>
public record PaymentResult(
    bool IsSuccess,
    string? ProviderTransactionId,
    string? ErrorMessage,
    object? ProviderResponse)
{
    public static PaymentResult Success(string providerTransactionId, object? response = null) =>
        new(true, providerTransactionId, null, response);

    public static PaymentResult Failure(string error, object? response = null) =>
        new(false, null, error, response);
}

/// <summary>Checkout session data returned to the frontend.</summary>
public record CheckoutData(
    Guid TransactionId,
    string Provider,
    bool IsDemo,
    string? CheckoutUrl,
    IReadOnlyDictionary<string, string>? SessionData);

/// <summary>Strategy pattern contract for all payment providers.</summary>
public interface IPaymentStrategy
{
    /// <summary>Starts a payment session and returns checkout data.</summary>
    Task<CheckoutData> InitiateAsync(PaymentRequest request, CancellationToken ct = default);

    /// <summary>Verifies a payment result with the provider.</summary>
    Task<PaymentResult> ProcessAsync(PaymentCallback callback, CancellationToken ct = default);

    /// <summary>Initiates a refund for a successful transaction.</summary>
    Task<PaymentResult> RefundAsync(string transactionId, CancellationToken ct = default);

    string GetProviderName();
}
