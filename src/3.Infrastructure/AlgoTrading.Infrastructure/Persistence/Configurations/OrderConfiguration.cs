using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// description(설명) : Order Entity Configuration (주문 엔티티 설정)
/// Details(상세설명) : EF Core configuration for Order entity mapping (Order 엔티티 매핑을 위한 EF Core 설정)
/// Applied technology patterns(적용기술패턴) : Fluent API Configuration, Entity Configuration Pattern (Fluent API 설정, 엔티티 설정 패턴)
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Table name
        builder.ToTable("Orders");

        // Primary key
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value))
            .IsRequired();

        // Stock Code (Value Object)
        builder.Property(o => o.StockCode)
            .HasConversion(
                sc => sc.Value,
                value => new StockCode(value))
            .HasMaxLength(6)
            .IsRequired();

        // Order properties
        builder.Property(o => o.Side)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.Quantity)
            .IsRequired();

        builder.Property(o => o.FilledQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(o => o.LimitPrice)
            .HasPrecision(18, 2);

        builder.Property(o => o.StopPrice)
            .HasPrecision(18, 2);

        builder.Property(o => o.AverageFillPrice)
            .HasPrecision(18, 2);

        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.TimeInForce)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.SubmittedAt)
            .IsRequired();

        builder.Property(o => o.AcceptedAt);

        builder.Property(o => o.CompletedAt);

        builder.Property(o => o.CancelledAt);

        builder.Property(o => o.BrokerOrderId)
            .HasMaxLength(50);

        builder.Property(o => o.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Source)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.StrategyId);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        builder.Property(o => o.RejectionReason)
            .HasMaxLength(500);

        // Indexes for query optimization
        builder.HasIndex(o => o.AccountNumber)
            .HasDatabaseName("IX_Orders_AccountNumber");

        builder.HasIndex(o => o.StockCode)
            .HasDatabaseName("IX_Orders_StockCode");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        builder.HasIndex(o => o.SubmittedAt)
            .HasDatabaseName("IX_Orders_SubmittedAt");

        builder.HasIndex(o => new { o.AccountNumber, o.Status })
            .HasDatabaseName("IX_Orders_AccountNumber_Status");

        builder.HasIndex(o => o.StrategyId)
            .HasDatabaseName("IX_Orders_StrategyId")
            .HasFilter("StrategyId IS NOT NULL");

        // Ignore navigation properties (domain events)
        builder.Ignore(o => o.DomainEvents);
    }
}
