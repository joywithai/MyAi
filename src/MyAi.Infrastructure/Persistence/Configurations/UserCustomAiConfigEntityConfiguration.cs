using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class UserCustomAiConfigEntityConfiguration : IEntityTypeConfiguration<UserCustomAiConfig>
{
    public void Configure(EntityTypeBuilder<UserCustomAiConfig> builder)
    {
        builder.ToTable("user_custom_ai_configs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(c => c.UserId, "idx_user_custom_ai_user_id").IsUnique();

        builder.Property(c => c.ProviderName).HasMaxLength(50).IsRequired();
        builder.Property(c => c.EncryptedApiKey).HasMaxLength(1000).IsRequired();
        builder.Property(c => c.PreferredModel).HasMaxLength(200);
        builder.Property(c => c.IsActive).HasColumnName("is_active");
        builder.Property(c => c.LastVerifiedAt).HasColumnName("last_verified_at");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserCustomAiConfig>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(c => c.DomainEvents);
    }
}
