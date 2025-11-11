using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// description(설명) : Trade Entity Configuration (거래 엔티티 설정)
/// Details(상세설명) : EF Core configuration for Trade entity mapping (Trade 엔티티 매핑을 위한 EF Core 설정)
/// Applied technology patterns(적용기술패턴) : Fluent API Configuration, Entity Configuration Pattern (Fluent API 설정, 엔티티 설정 패턴)
/// </summary>
public sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        // Table name
        builder.ToTable("Trades");

        // Primary key
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => new TradeId(value))
            .IsRequired();

        // Stock Code (Value Object)
        builder.Property(t => t.StockCode)
            .HasConversion(
                sc => sc.Value,
                value => new StockCode(value))
            .HasMaxLength(6)
            .IsRequired();

        // Trade properties
        builder.Property(t => t.Side)
            .HasConversion<int>()
            .IsRequired();

        // Entry Order ID (Value Object)
        builder.Property(t => t.EntryOrderId)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value))
            .IsRequired();

        // Exit Order ID (Value Object)
        builder.Property(t => t.ExitOrderId)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value))
            .IsRequired();

        builder.Property(t => t.Quantity)
            .IsRequired();

        builder.Property(t => t.EntryPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.ExitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.EntryTime)
            .IsRequired();

        builder.Property(t => t.ExitTime)
            .IsRequired();

        // Realized P/L (Value Object - Money)
        builder.OwnsOne(t => t.RealizedPL, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("RealizedPL")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Property(t => t.RealizedPLPercent)
            .HasPrecision(18, 4)
            .IsRequired();

        // Commission (Value Object - Money)
        builder.OwnsOne(t => t.Commission, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Commission")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Property(t => t.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.StrategyId);

        builder.Property(t => t.Result)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        // Indexes for query optimization
        builder.HasIndex(t => t.AccountNumber)
            .HasDatabaseName("IX_Trades_AccountNumber");

        builder.HasIndex(t => t.StockCode)
            .HasDatabaseName("IX_Trades_StockCode");

        builder.HasIndex(t => t.ExitTime)
            .HasDatabaseName("IX_Trades_ExitTime");

        builder.HasIndex(t => t.Result)
            .HasDatabaseName("IX_Trades_Result");

        builder.HasIndex(t => new { t.AccountNumber, t.ExitTime })
            .HasDatabaseName("IX_Trades_AccountNumber_ExitTime");

        builder.HasIndex(t => t.StrategyId)
            .HasDatabaseName("IX_Trades_StrategyId")
            .HasFilter("StrategyId IS NOT NULL");

        // Ignore calculated properties
        builder.Ignore(t => t.Duration);
    }
}
