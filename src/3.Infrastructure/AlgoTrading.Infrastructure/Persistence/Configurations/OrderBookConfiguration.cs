using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for OrderBook entity
/// Maps to market.order_book table
/// </summary>
public class OrderBookConfiguration : IEntityTypeConfiguration<OrderBook>
{
    public void Configure(EntityTypeBuilder<OrderBook> builder)
    {
        // Table configuration
        builder.ToTable("order_book", "market");

        // Primary key
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new OrderBookId(value))
            .IsRequired();

        builder.Property(o => o.StockCode)
            .HasColumnName("stock_code")
            .HasConversion(
                code => code.Value,
                value => new StockCode(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CapturedAt)
            .HasColumnName("captured_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Ask Price/Volume configurations (10 levels)
        ConfigureAskLevel(builder, 1);
        ConfigureAskLevel(builder, 2);
        ConfigureAskLevel(builder, 3);
        ConfigureAskLevel(builder, 4);
        ConfigureAskLevel(builder, 5);
        ConfigureAskLevel(builder, 6);
        ConfigureAskLevel(builder, 7);
        ConfigureAskLevel(builder, 8);
        ConfigureAskLevel(builder, 9);
        ConfigureAskLevel(builder, 10);

        // Bid Price/Volume configurations (10 levels)
        ConfigureBidLevel(builder, 1);
        ConfigureBidLevel(builder, 2);
        ConfigureBidLevel(builder, 3);
        ConfigureBidLevel(builder, 4);
        ConfigureBidLevel(builder, 5);
        ConfigureBidLevel(builder, 6);
        ConfigureBidLevel(builder, 7);
        ConfigureBidLevel(builder, 8);
        ConfigureBidLevel(builder, 9);
        ConfigureBidLevel(builder, 10);

        builder.Property(o => o.TotalAskVolume)
            .HasColumnName("total_ask_volume");

        builder.Property(o => o.TotalBidVolume)
            .HasColumnName("total_bid_volume");

        // Index
        builder.HasIndex(o => new { o.StockCode, o.CapturedAt })
            .HasDatabaseName("idx_orderbook_stock_time")
            .IsDescending(false, true); // captured_at DESC
    }

    private void ConfigureAskLevel(EntityTypeBuilder<OrderBook> builder, int level)
    {
        var priceProperty = typeof(OrderBook).GetProperty($"AskPrice{level}");
        var volumeProperty = typeof(OrderBook).GetProperty($"AskVolume{level}");

        if (priceProperty != null)
        {
            builder.OwnsOne(priceProperty.PropertyType, $"AskPrice{level}", money =>
            {
                money.Property("Amount")
                    .HasColumnName($"ask_price_{level}")
                    .HasColumnType("decimal(18,2)");

                money.Property("Currency")
                    .HasConversion<string>()
                    .HasMaxLength(3)
                    .IsRequired(false);
            });
        }

        if (volumeProperty != null)
        {
            builder.Property(volumeProperty.Name)
                .HasColumnName($"ask_volume_{level}");
        }
    }

    private void ConfigureBidLevel(EntityTypeBuilder<OrderBook> builder, int level)
    {
        var priceProperty = typeof(OrderBook).GetProperty($"BidPrice{level}");
        var volumeProperty = typeof(OrderBook).GetProperty($"BidVolume{level}");

        if (priceProperty != null)
        {
            builder.OwnsOne(priceProperty.PropertyType, $"BidPrice{level}", money =>
            {
                money.Property("Amount")
                    .HasColumnName($"bid_price_{level}")
                    .HasColumnType("decimal(18,2)");

                money.Property("Currency")
                    .HasConversion<string>()
                    .HasMaxLength(3)
                    .IsRequired(false);
            });
        }

        if (volumeProperty != null)
        {
            builder.Property(volumeProperty.Name)
                .HasColumnName($"bid_volume_{level}");
        }
    }
}
