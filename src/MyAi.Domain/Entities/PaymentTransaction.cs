using System.Text.Json;
using MyAi.Domain.Enums;
using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class PaymentTransaction : Entity, IAuditableEntity
{
    private PaymentTransaction() { } // EF Core

    private PaymentTransaction(
        Guid userId, Guid planId, decimal amount, string currency, PaymentProvider provider)
    {
        UserId = userId;
        PlanId = planId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
        PaymentProvider = provider;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public Guid PlanId { get; private set; }

    public Guid? SubscriptionId { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public PaymentStatus Status { get; private set; }

    public PaymentProvider PaymentProvider { get; private set; }

    public string? ProviderTransactionId { get; private set; }

    public string? ProviderResponse { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static PaymentTransaction Create(
        Guid userId, Guid planId, decimal amount, string currency, PaymentProvider provider)
        => new(userId, planId, amount, currency, provider);

    public void AttachSubscription(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSuccess(string providerTransactionId, object? providerResponse)
    {
        Status = PaymentStatus.Success;
        ProviderTransactionId = providerTransactionId;
        ProviderResponse = providerResponse is null ? null : JsonSerializer.Serialize(providerResponse, SegmentJsonOptions);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(object? providerResponse)
    {
        Status = PaymentStatus.Failed;
        ProviderResponse = providerResponse is null ? null : JsonSerializer.Serialize(providerResponse, SegmentJsonOptions);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRefunded()
    {
        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public static readonly JsonSerializerOptions SegmentJsonOptions = new(JsonSerializerDefaults.Web);
}
