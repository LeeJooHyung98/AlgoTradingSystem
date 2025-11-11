using AlgoTrading.Core.Entities.Monitoring;
using AlgoTrading.Core.Entities.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for TradingMetric entity
/// Maps to monitoring.trading_metrics table (TimescaleDB Hypertable)
/// </summary>
public class TradingMetricConfiguration : IEntityTypeConfiguration<TradingMetric>
{
    public void Configure(EntityTypeBuilder<TradingMetric> builder)
    {
        // Table configuration - TimescaleDB Hypertable
        builder.ToTable("trading_metrics", "monitoring");

        // Composite primary key (account_id, metric_timestamp)
        builder.HasKey(t => new { t.AccountId, t.MetricTimestamp });

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new TradingMetricId(value))
            .IsRequired();

        builder.Property(t => t.AccountId)
            .HasColumnName("account_id")
            .HasConversion(
                id => id.Value,
                value => new AccountId(value))
            .IsRequired();

        builder.Property(t => t.MetricTimestamp)
            .HasColumnName("metric_timestamp")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.OrdersSubmitted)
            .HasColumnName("orders_submitted")
            .IsRequired();

        builder.Property(t => t.OrdersFilled)
            .HasColumnName("orders_filled")
            .IsRequired();

        builder.Property(t => t.OrdersCancelled)
            .HasColumnName("orders_cancelled")
            .IsRequired();

        builder.Property(t => t.OrdersRejected)
            .HasColumnName("orders_rejected")
            .IsRequired();

        // Money property - TotalExecutionAmount
        builder.OwnsOne(t => t.TotalExecutionAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("total_execution_amount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(t => t.AvgExecutionTimeMs)
            .HasColumnName("avg_execution_time_ms")
            .HasColumnType("decimal(10,2)");

        builder.Property(t => t.OpenPositions)
            .HasColumnName("open_positions")
            .IsRequired();

        builder.Property(t => t.ClosedPositions)
            .HasColumnName("closed_positions")
            .IsRequired();

        // Money property - RealizedPl
        builder.OwnsOne(t => t.RealizedPl, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("realized_pl")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money property - UnrealizedPl
        builder.OwnsOne(t => t.UnrealizedPl, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("unrealized_pl")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Index
        builder.HasIndex(t => new { t.AccountId, t.MetricTimestamp })
            .HasDatabaseName("idx_trading_metrics_account")
            .IsDescending(false, true); // metric_timestamp DESC

        // Note: TimescaleDB Hypertable conversion is done via migration, not in configuration
        // Foreign keys will be added in migration
    }
}
