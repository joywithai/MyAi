using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.Token).HasMaxLength(500).IsRequired();
        builder.HasIndex(t => t.Token, "idx_refresh_tokens_token").IsUnique();
        builder.HasIndex(t => t.UserId, "idx_refresh_tokens_user_id");
        builder.HasIndex(t => t.ExpiresAt, "idx_refresh_tokens_expires");

        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at");
        builder.Property(t => t.IsRevoked).HasColumnName("is_revoked");
        builder.Property(t => t.RevokedAt).HasColumnName("revoked_at");
        builder.Property(t => t.CreatedByIp).HasMaxLength(50).HasColumnName("created_by_ip");
        builder.Property(t => t.ReplacedByToken).HasMaxLength(500).HasColumnName("replaced_by_token");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(t => t.DomainEvents);
    }
}
