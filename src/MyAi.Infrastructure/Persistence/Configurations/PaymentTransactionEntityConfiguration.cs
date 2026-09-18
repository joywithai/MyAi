using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class PaymentTransactionEntityConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(t => t.UserId, "idx_payment_transactions_user_id");

        builder.Property(t => t.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(t => t.SubscriptionId).HasColumnName("subscription_id");

        builder.Property(t => t.Amount).HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(t => t.Currency).HasMaxLength(10).IsRequired();

        builder.Property(t => t.Status)
            .HasMaxLength(20)
            .HasConversion(
                s => s switch
                {
                    PaymentStatus.Success => "success",
                    PaymentStatus.Failed => "failed",
                    PaymentStatus.Refunded => "refunded",
                    _ => "pending"
                },
                v => v switch
                {
                    "success" => PaymentStatus.Success,
                    "failed" => PaymentStatus.Failed,
                    "refunded" => PaymentStatus.Refunded,
                    _ => PaymentStatus.Pending
                })
            .IsRequired();
        builder.HasIndex(t => t.Status, "idx_payment_transactions_status");

        builder.Property(t => t.PaymentProvider)
            .HasColumnName("payment_provider")
            .HasMaxLength(50)
            .HasConversion(
                p => p switch
                {
                    PaymentProvider.Stripe => "stripe",
                    PaymentProvider.SslCommerz => "sslcommerz",
                    _ => "demo"
                },
                v => v switch
                {
                    "stripe" => PaymentProvider.Stripe,
                    "sslcommerz" => PaymentProvider.SslCommerz,
                    _ => PaymentProvider.Demo
                })
            .IsRequired();

        builder.Property(t => t.ProviderTransactionId).HasMaxLength(500).HasColumnName("provider_transaction_id");
        builder.HasIndex(t => t.ProviderTransactionId, "idx_payment_transactions_provider_tid");

        builder.Property(t => t.ProviderResponse).HasColumnName("provider_response").HasColumnType("jsonb");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SubscriptionPlan>()
            .WithMany()
            .HasForeignKey(t => t.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(t => t.DomainEvents);
    }
}
