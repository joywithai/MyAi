using System.ComponentModel.DataAnnotations;

namespace MyAi.Api.Contracts;

public class InitiatePaymentRequest
{
    [Required]
    public Guid PlanId { get; set; }
}

public class ProcessPaymentRequest
{
    [Required]
    public Guid TransactionId { get; set; }

    public string? ProviderTransactionId { get; set; }

    public Dictionary<string, string>? ProviderData { get; set; }
}
