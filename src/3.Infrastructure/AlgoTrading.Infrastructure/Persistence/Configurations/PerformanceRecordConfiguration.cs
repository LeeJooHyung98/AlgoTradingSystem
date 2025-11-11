using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PerformanceRecord entity
/// Maps to account.performance_records table (TimescaleDB Hypertable)
/// </summary>
public class PerformanceRecordConfiguration : IEntityTypeConfiguration<PerformanceRecord>
{
    public void Configure(EntityTypeBuilder<PerformanceRecord> builder)
    {
        // Table configuration - TimescaleDB Hypertable
        builder.ToTable("performance_records", "account");

        // Composite primary key (account_id, record_date)
        builder.HasKey(p => new { p.AccountId, p.RecordDate });

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new PerformanceRecordId(value))
            .IsRequired();

        builder.Property(p => p.AccountId)
            .HasColumnName("account_id")
            .HasConversion(
                id => id.Value,
                value => new AccountId(value))
            .IsRequired();

        builder.Property(p => p.RecordDate)
            .HasColumnName("record_date")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Money properties - EquityValue
        builder.OwnsOne(p => p.EquityValue, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("equity_value")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - DailyReturn
        builder.OwnsOne(p => p.DailyReturn, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("daily_return")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(p => p.DailyReturnPercent)
            .HasColumnName("daily_return_percent")
            .HasColumnType("decimal(10,4)")
            .IsRequired();

        // Money properties - CumulativeReturn (nullable)
        builder.OwnsOne(p => p.CumulativeReturn, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("cumulative_return")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(p => p.CumulativeReturnPercent)
            .HasColumnName("cumulative_return_percent")
            .HasColumnType("decimal(10,4)");

        builder.Property(p => p.SharpeRatio)
            .HasColumnName("sharpe_ratio")
            .HasColumnType("decimal(10,4)");

        builder.Property(p => p.MaxDrawdown)
            .HasColumnName("max_drawdown")
            .HasColumnType("decimal(10,4)");

        builder.Property(p => p.WinRate)
            .HasColumnName("win_rate")
            .HasColumnType("decimal(10,4)");

        builder.Property(p => p.RecordedAt)
            .HasColumnName("recorded_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Index
        builder.HasIndex(p => new { p.AccountId, p.RecordDate })
            .HasDatabaseName("idx_performance_account")
            .IsDescending(false, true); // record_date DESC
    }
}
