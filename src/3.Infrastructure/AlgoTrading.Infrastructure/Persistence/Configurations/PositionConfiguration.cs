using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// description(설명) : Position Entity Configuration (포지션 엔티티 설정)
/// Details(상세설명) : EF Core configuration for Position entity mapping (Position 엔티티 매핑을 위한 EF Core 설정)
/// Applied technology patterns(적용기술패턴) : Fluent API Configuration, Entity Configuration Pattern (Fluent API 설정, 엔티티 설정 패턴)
/// </summary>
public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        // Table name
        builder.ToTable("Positions");

        // Primary key
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new PositionId(value))
            .IsRequired();

        // Stock Code (Value Object)
        builder.Property(p => p.StockCode)
            .HasConversion(
                sc => sc.Value,
                value => new StockCode(value))
            .HasMaxLength(6)
            .IsRequired();

        // Position properties
        builder.Property(p => p.Side)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Quantity)
            .IsRequired();

        builder.Property(p => p.AverageEntryPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.CurrentPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        // Entry Order ID (Value Object)
        builder.Property(p => p.EntryOrderId)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value))
            .IsRequired();

        builder.Property(p => p.StopLossPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.TakeProfitPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.OpenedAt)
            .IsRequired();

        builder.Property(p => p.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.StrategyId);

        // Indexes for query optimization
        builder.HasIndex(p => p.AccountNumber)
            .HasDatabaseName("IX_Positions_AccountNumber");

        builder.HasIndex(p => p.StockCode)
            .HasDatabaseName("IX_Positions_StockCode");

        builder.HasIndex(p => new { p.AccountNumber, p.StockCode })
            .HasDatabaseName("IX_Positions_AccountNumber_StockCode")
            .IsUnique();

        builder.HasIndex(p => p.StrategyId)
            .HasDatabaseName("IX_Positions_StrategyId")
            .HasFilter("StrategyId IS NOT NULL");

        builder.HasIndex(p => p.OpenedAt)
            .HasDatabaseName("IX_Positions_OpenedAt");

        // Ignore calculated properties and domain events
        builder.Ignore(p => p.CostBasis);
        builder.Ignore(p => p.MarketValue);
        builder.Ignore(p => p.UnrealizedPL);
        builder.Ignore(p => p.UnrealizedPLPercent);
        builder.Ignore(p => p.DomainEvents);
    }
}
