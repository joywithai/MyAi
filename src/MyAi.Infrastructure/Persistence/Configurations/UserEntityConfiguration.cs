using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .HasConversion(e => e.Value, v => Domain.ValueObjects.Email.Create(v))
            .IsRequired();

        builder.HasIndex(u => u.Email, "idx_users_email").IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .HasColumnName("display_name")
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion(r => r.ToString().ToLowerInvariant(), v => ParseRole(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(u => u.Role, "idx_users_role");

        builder.Property(u => u.Status)
            .HasConversion(s => s.ToString().ToLowerInvariant(), v => ParseStatus(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(u => u.Status, "idx_users_status");

        builder.Property(u => u.EmailVerified).HasColumnName("email_verified");
        builder.Property(u => u.EmailVerifiedAt).HasColumnName("email_verified_at");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");
        builder.Property(u => u.LastLoginAt).HasColumnName("last_login_at");

        builder.Ignore(u => u.DomainEvents);
    }

    internal static UserRole ParseRole(string value) => value switch
    {
        "admin" => UserRole.Admin,
        "subscriber" => UserRole.Subscriber,
        _ => UserRole.PublicUser
    };

    internal static UserStatus ParseStatus(string value) => value switch
    {
        "inactive" => UserStatus.Inactive,
        "banned" => UserStatus.Banned,
        _ => UserStatus.Active
    };
}
