using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for StockInfo entity
/// Maps to market.stock_info table
/// </summary>
public class StockInfoConfiguration : IEntityTypeConfiguration<StockInfo>
{
    public void Configure(EntityTypeBuilder<StockInfo> builder)
    {
        // Table configuration
        builder.ToTable("stock_info", "market");

        // Primary key - StockCode value object
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("stock_code")
            .HasConversion(
                id => id.Value,
                value => new StockCode(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.StockName)
            .HasColumnName("stock_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.MarketType)
            .HasColumnName("market_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Sector)
            .HasColumnName("sector")
            .HasMaxLength(50);

        builder.Property(s => s.Industry)
            .HasColumnName("industry")
            .HasMaxLength(50);

        builder.Property(s => s.ListingDate)
            .HasColumnName("listing_date")
            .HasColumnType("date");

        builder.Property(s => s.ListedShares)
            .HasColumnName("listed_shares");

        builder.Property(s => s.IsTradable)
            .HasColumnName("is_tradable")
            .IsRequired();

        builder.Property(s => s.IsSuspended)
            .HasColumnName("is_suspended")
            .IsRequired();

        // Money properties - UpperLimitPrice
        builder.OwnsOne(s => s.UpperLimitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("upper_limit_price")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // Money properties - LowerLimitPrice
        builder.OwnsOne(s => s.LowerLimitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("lower_limit_price")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired(false);
        });

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        // Indexes
        builder.HasIndex(s => s.MarketType)
            .HasDatabaseName("idx_stock_info_market");

        builder.HasIndex(s => s.Sector)
            .HasDatabaseName("idx_stock_info_sector");
    }
}
