using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AccountBalanceHistory entity
/// Maps to account.account_balance_history table (TimescaleDB Hypertable)
/// </summary>
public class AccountBalanceHistoryConfiguration : IEntityTypeConfiguration<AccountBalanceHistory>
{
    public void Configure(EntityTypeBuilder<AccountBalanceHistory> builder)
    {
        // Table configuration - TimescaleDB Hypertable
        builder.ToTable("account_balance_history", "account");

        // Composite primary key (account_id, balance_date)
        builder.HasKey(h => new { h.AccountId, h.BalanceDate });

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new AccountBalanceHistoryId(value))
            .IsRequired();

        builder.Property(h => h.AccountId)
            .HasColumnName("account_id")
            .HasConversion(
                id => id.Value,
                value => new AccountId(value))
            .IsRequired();

        builder.Property(h => h.BalanceDate)
            .HasColumnName("balance_date")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Money properties - CashBalance
        builder.OwnsOne(h => h.CashBalance, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("cash_balance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - TotalAssets
        builder.OwnsOne(h => h.TotalAssets, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("total_assets")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - TotalEvaluation
        builder.OwnsOne(h => h.TotalEvaluation, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("total_evaluation")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - TotalProfitLoss
        builder.OwnsOne(h => h.TotalProfitLoss, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("total_pl")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(h => h.RecordedAt)
            .HasColumnName("recorded_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Index
        builder.HasIndex(h => new { h.AccountId, h.BalanceDate })
            .HasDatabaseName("idx_account_balance_account")
            .IsDescending(false, true); // balance_date DESC

        // Note: TimescaleDB Hypertable conversion is done via migration, not in configuration
    }
}
