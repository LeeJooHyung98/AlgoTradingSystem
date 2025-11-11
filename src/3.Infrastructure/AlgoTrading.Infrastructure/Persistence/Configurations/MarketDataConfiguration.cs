using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for MarketData entity
/// Maps to market.market_data table (TimescaleDB Hypertable)
/// </summary>
public class MarketDataConfiguration : IEntityTypeConfiguration<MarketData>
{
    public void Configure(EntityTypeBuilder<MarketData> builder)
    {
        // Table configuration - TimescaleDB Hypertable
        builder.ToTable("market_data", "market");

        // Composite primary key (stock_code, data_timestamp)
        builder.HasKey(m => new { m.StockCode, m.DataTimestamp });

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new MarketDataId(value))
            .IsRequired();

        builder.Property(m => m.StockCode)
            .HasColumnName("stock_code")
            .HasConversion(
                code => code.Value,
                value => new StockCode(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.DataTimestamp)
            .HasColumnName("data_timestamp")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Money properties - OpenPrice
        builder.OwnsOne(m => m.OpenPrice, money =>
        {
            money.Property(p => p.Amount)
                .HasColumnName("open_price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(p => p.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - HighPrice
        builder.OwnsOne(m => m.HighPrice, money =>
        {
            money.Property(p => p.Amount)
                .HasColumnName("high_price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(p => p.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - LowPrice
        builder.OwnsOne(m => m.LowPrice, money =>
        {
            money.Property(p => p.Amount)
                .HasColumnName("low_price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(p => p.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - ClosePrice
        builder.OwnsOne(m => m.ClosePrice, money =>
        {
            money.Property(p => p.Amount)
                .HasColumnName("close_price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(p => p.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(m => m.Volume)
            .HasColumnName("volume")
            .IsRequired();

        builder.Property(m => m.TradeCount)
            .HasColumnName("trade_count");

        // Money properties - ChangeAmount (nullable)
        builder.OwnsOne(m => m.ChangeAmount, money =>
        {
            money.Property(p => p.Amount)
                .HasColumnName("change_amount")
                .HasColumnType("decimal(18,2)");

            money.Property(p => p.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(m => m.ChangePercent)
            .HasColumnName("change_percent")
            .HasColumnType("decimal(10,4)");

        builder.Property(m => m.MarketStatus)
            .HasColumnName("market_status")
            .HasConversion<string>()
            .HasMaxLength(20);

        // Index
        builder.HasIndex(m => new { m.StockCode, m.DataTimestamp })
            .HasDatabaseName("idx_market_data_stock")
            .IsDescending(false, true); // data_timestamp DESC

        // Note: TimescaleDB Hypertable conversion is done via migration, not in configuration
    }
}
