using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for TickData entity
/// Maps to market.tick_data table (TimescaleDB Hypertable)
/// </summary>
public class TickDataConfiguration : IEntityTypeConfiguration<TickData>
{
    public void Configure(EntityTypeBuilder<TickData> builder)
    {
        // Table configuration - TimescaleDB Hypertable
        builder.ToTable("tick_data", "market");

        // Composite primary key (stock_code, tick_timestamp)
        builder.HasKey(t => new { t.StockCode, t.TickTimestamp });

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new TickDataId(value))
            .IsRequired();

        builder.Property(t => t.StockCode)
            .HasColumnName("stock_code")
            .HasConversion(
                code => code.Value,
                value => new StockCode(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.TickTimestamp)
            .HasColumnName("tick_timestamp")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Money property - ExecutionPrice
        builder.OwnsOne(t => t.ExecutionPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("execution_price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(t => t.ExecutionVolume)
            .HasColumnName("execution_volume")
            .IsRequired();

        builder.Property(t => t.ExecutionSide)
            .HasColumnName("execution_side")
            .HasMaxLength(10)
            .IsRequired();

        // Index
        builder.HasIndex(t => new { t.StockCode, t.TickTimestamp })
            .HasDatabaseName("idx_tick_stock_time")
            .IsDescending(false, true); // tick_timestamp DESC

        // Note: TimescaleDB Hypertable conversion is done via migration, not in configuration
    }
}
