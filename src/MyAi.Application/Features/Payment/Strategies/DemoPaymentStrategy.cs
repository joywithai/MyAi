namespace MyAi.Application.Features.Payment.Strategies;

/// <summary>
/// Demo payment: clicking checkout always succeeds. No real money, no card data.
/// Same interface as real providers so switching is a config change.
/// </summary>
public class DemoPaymentStrategy : IPaymentStrategy
{
    public Task<CheckoutData> InitiateAsync(PaymentRequest request, CancellationToken ct = default)
    {
        var sessionId = $"demo_{Guid.NewGuid():N}";
        var sessionData = new Dictionary<string, string>
        {
            ["session_id"] = sessionId,
            ["amount"] = request.Amount.ToString("F2"),
            ["currency"] = request.Currency
        };

        var checkoutUrl = $"/payment/checkout?transactionId={request.TransactionId}&session={sessionId}";

        return Task.FromResult(new CheckoutData(
            request.TransactionId, GetProviderName(), IsDemo: true, checkoutUrl, sessionData));
    }

    public Task<PaymentResult> ProcessAsync(PaymentCallback callback, CancellationToken ct = default)
    {
        // Demo mode: always succeeds.
        return Task.FromResult(PaymentResult.Success(
            callback.ProviderTransactionId ?? $"demo_txn_{Guid.NewGuid():N}",
            new { mode = "demo", verified = true }));
    }

    public Task<PaymentResult> RefundAsync(string transactionId, CancellationToken ct = default) =>
        Task.FromResult(PaymentResult.Success(transactionId, new { mode = "demo", refunded = true }));

    public string GetProviderName() => "demo";
}
