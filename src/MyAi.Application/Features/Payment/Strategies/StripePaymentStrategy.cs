namespace MyAi.Application.Features.Payment.Strategies;

/// <summary>
/// Stripe integration skeleton — structure ready, real implementation pending.
/// PCI-DSS: card data never touches our server; Stripe hosted checkout is used.
/// </summary>
public class StripePaymentStrategy : IPaymentStrategy
{
    public Task<CheckoutData> InitiateAsync(PaymentRequest request, CancellationToken ct = default)
    {
        // TODO: POST /v1/checkout/sessions with line_items, success_url, cancel_url.
        // Return session.url as CheckoutUrl once the Stripe SDK is wired.
        throw new NotSupportedException(
            "Stripe payments are not enabled yet. Use the demo provider (Payments:Provider=demo).");
    }

    public Task<PaymentResult> ProcessAsync(PaymentCallback callback, CancellationToken ct = default)
    {
        // TODO: verify webhook signature (checkout.session.completed), then fetch session status.
        throw new NotSupportedException("Stripe payment processing is not enabled yet.");
    }

    public Task<PaymentResult> RefundAsync(string transactionId, CancellationToken ct = default)
    {
        // TODO: POST /v1/refunds with payment_intent.
        throw new NotSupportedException("Stripe refunds are not enabled yet.");
    }

    public string GetProviderName() => "stripe";
}
