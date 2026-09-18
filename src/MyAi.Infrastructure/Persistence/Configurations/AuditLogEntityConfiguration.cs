using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class AuditLogEntityConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.HasIndex(a => a.UserId, "idx_audit_logs_user_id");

        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.HasIndex(a => a.Action, "idx_audit_logs_action");

        builder.Property(a => a.EntityType).HasMaxLength(100).HasColumnName("entity_type");
        builder.Property(a => a.EntityId).HasColumnName("entity_id");
        builder.HasIndex(a => new { a.EntityType, a.EntityId }, "idx_audit_logs_entity");

        builder.Property(a => a.OldValues).HasColumnName("old_values").HasColumnType("jsonb");
        builder.Property(a => a.NewValues).HasColumnName("new_values").HasColumnType("jsonb");
        builder.Property(a => a.IpAddress).HasMaxLength(50).HasColumnName("ip_address");
        builder.Property(a => a.UserAgent).HasMaxLength(500).HasColumnName("user_agent");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(a => a.CreatedAt, "idx_audit_logs_created_at");

        builder.Ignore(a => a.DomainEvents);
    }
}
