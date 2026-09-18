namespace MyAi.Domain.Events;

public class PaymentCompletedEvent : DomainEvent
{
    public PaymentCompletedEvent(Guid transactionId, Guid userId, Guid planId, decimal amount, string provider)
    {
        TransactionId = transactionId;
        UserId = userId;
        PlanId = planId;
        Amount = amount;
        Provider = provider;
    }

    public Guid TransactionId { get; }
    public Guid UserId { get; }
    public Guid PlanId { get; }
    public decimal Amount { get; }
    public string Provider { get; }
}
