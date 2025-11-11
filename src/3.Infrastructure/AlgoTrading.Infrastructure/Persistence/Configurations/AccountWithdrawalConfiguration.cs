using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AccountWithdrawal entity
/// Maps to account.account_withdrawals table
/// </summary>
public class AccountWithdrawalConfiguration : IEntityTypeConfiguration<AccountWithdrawal>
{
    public void Configure(EntityTypeBuilder<AccountWithdrawal> builder)
    {
        // Table configuration
        builder.ToTable("account_withdrawals", "account");

        // Primary key
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new AccountWithdrawalId(value))
            .IsRequired();

        builder.Property(w => w.AccountId)
            .HasColumnName("account_id")
            .HasConversion(
                id => id.Value,
                value => new AccountId(value))
            .IsRequired();

        // Money properties - Amount
        builder.OwnsOne(w => w.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(w => w.BankCode)
            .HasColumnName("bank_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(w => w.BankAccountNumber)
            .HasColumnName("bank_account_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(w => w.AccountHolderName)
            .HasColumnName("account_holder_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.Status)
            .HasColumnName("withdrawal_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(w => w.ApprovedAt)
            .HasColumnName("approved_at")
            .HasColumnType("timestamptz");

        builder.Property(w => w.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz");

        builder.Property(w => w.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamptz");

        builder.Property(w => w.FailureReason)
            .HasColumnName("failure_reason")
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(w => w.AccountId)
            .HasDatabaseName("idx_withdrawals_account");

        builder.HasIndex(w => w.Status)
            .HasDatabaseName("idx_withdrawals_status");

        // Check constraint
        builder.HasCheckConstraint(
            "chk_withdrawal_amount",
            "amount > 0");

        // Foreign keys will be added in migration
    }
}
