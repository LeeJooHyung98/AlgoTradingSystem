using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AccountRestriction entity
/// Maps to account.account_restrictions table
/// </summary>
public class AccountRestrictionConfiguration : IEntityTypeConfiguration<AccountRestriction>
{
    public void Configure(EntityTypeBuilder<AccountRestriction> builder)
    {
        // Table configuration
        builder.ToTable("account_restrictions", "account");

        // Primary key
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new AccountRestrictionId(value))
            .IsRequired();

        builder.Property(r => r.AccountId)
            .HasColumnName("account_id")
            .HasConversion(
                id => id.Value,
                value => new AccountId(value))
            .IsRequired();

        builder.Property(r => r.Type)
            .HasColumnName("restriction_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("restriction_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Reason)
            .HasColumnName("reason")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(r => r.AppliedAt)
            .HasColumnName("applied_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.Property(r => r.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz");

        builder.Property(r => r.AppliedBy)
            .HasColumnName("applied_by")
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(r => r.RevokedBy)
            .HasColumnName("revoked_by")
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        // Indexes
        builder.HasIndex(r => r.AccountId)
            .HasDatabaseName("idx_account_restrictions_account");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("idx_account_restrictions_status");

        // Foreign keys will be added in migration
    }
}
