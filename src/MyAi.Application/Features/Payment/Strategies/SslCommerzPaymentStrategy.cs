namespace MyAi.Application.Features.Payment.Strategies;

/// <summary>
/// SSLCommerz integration skeleton — structure ready, real implementation pending.
/// Uses the SSLCommerz hosted payment page; IPN validation on callback.
/// </summary>
public class SslCommerzPaymentStrategy : IPaymentStrategy
{
    public Task<CheckoutData> InitiateAsync(PaymentRequest request, CancellationToken ct = default)
    {
        // TODO: POST /gwprocess/v4/api.php (session request) → return GatewayPageURL.
        throw new NotSupportedException(
            "SSLCommerz payments are not enabled yet. Use the demo provider (Payments:Provider=demo).");
    }

    public Task<PaymentResult> ProcessAsync(PaymentCallback callback, CancellationToken ct = default)
    {
        // TODO: validate IPN hash + call /validator/api/validationserverAPI.php.
        throw new NotSupportedException("SSLCommerz payment processing is not enabled yet.");
    }

    public Task<PaymentResult> RefundAsync(string transactionId, CancellationToken ct = default)
    {
        // TODO: POST /merchant/refund/api.php with bank_tran_id.
        throw new NotSupportedException("SSLCommerz refunds are not enabled yet.");
    }

    public string GetProviderName() => "sslcommerz";
}
