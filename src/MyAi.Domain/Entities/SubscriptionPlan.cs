using MyAi.Domain.Enums;
using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class SubscriptionPlan : Entity, IAuditableEntity
{
    private SubscriptionPlan() { } // EF Core

    private SubscriptionPlan(
        string name, UserRole roleGranted, BillingCycle billingCycle, decimal priceAmount, string currency)
    {
        Name = name;
        RoleGranted = roleGranted;
        BillingCycle = billingCycle;
        PriceAmount = priceAmount;
        PriceCurrency = currency;
        Features = new List<string>();
        IsActive = true;
        SortOrder = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; }

    public UserRole RoleGranted { get; private set; }

    public BillingCycle BillingCycle { get; private set; }

    public decimal PriceAmount { get; private set; }

    public string PriceCurrency { get; private set; }

    public string? Description { get; private set; }

    public List<string> Features { get; private set; }

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public static SubscriptionPlan Create(
        string name, UserRole roleGranted, BillingCycle cycle, decimal price, string currency = "BDT")
    {
        if (price < 0)
        {
            throw new ArgumentException("Plan price cannot be negative.");
        }

        return new SubscriptionPlan(name, roleGranted, cycle, price, currency);
    }

    public void SetDescription(string? description) => Description = description;

    public void SetFeatures(List<string> features) => Features = features;

    public void SetSortOrder(int order) => SortOrder = order;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public DateTime CalculateExpiryDate(DateTime startDate) => BillingCycle switch
    {
        BillingCycle.Yearly => startDate.AddDays(365),
        _ => startDate.AddDays(30)
    };
}
