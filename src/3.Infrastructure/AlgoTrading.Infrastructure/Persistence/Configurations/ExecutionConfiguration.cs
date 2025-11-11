using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// description(설명) : Execution Entity Configuration (체결 엔티티 설정)
/// Details(상세설명) : EF Core configuration for Execution entity mapping (Execution 엔티티 매핑을 위한 EF Core 설정)
/// Applied technology patterns(적용기술패턴) : Fluent API Configuration, Entity Configuration Pattern (Fluent API 설정, 엔티티 설정 패턴)
/// </summary>
public sealed class ExecutionConfiguration : IEntityTypeConfiguration<Execution>
{
    public void Configure(EntityTypeBuilder<Execution> builder)
    {
        // Table name
        builder.ToTable("Executions");

        // Primary key
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => new ExecutionId(value))
            .IsRequired();

        // Order ID (Value Object)
        builder.Property(e => e.OrderId)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value))
            .IsRequired();

        // Stock Code (Value Object)
        builder.Property(e => e.StockCode)
            .HasConversion(
                sc => sc.Value,
                value => new StockCode(value))
            .HasMaxLength(6)
            .IsRequired();

        // Execution properties
        builder.Property(e => e.Side)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        // Commission (Value Object - Money)
        builder.OwnsOne(e => e.Commission, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Commission")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Property(e => e.ExecutionTime)
            .IsRequired();

        builder.Property(e => e.BrokerExecutionId)
            .HasMaxLength(50);

        builder.Property(e => e.ExecutionVenue)
            .HasMaxLength(50);

        builder.Property(e => e.IsPartialFill)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();

        // Indexes for query optimization
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("IX_Executions_OrderId");

        builder.HasIndex(e => e.AccountNumber)
            .HasDatabaseName("IX_Executions_AccountNumber");

        builder.HasIndex(e => e.ExecutionTime)
            .HasDatabaseName("IX_Executions_ExecutionTime");

        builder.HasIndex(e => e.BrokerExecutionId)
            .HasDatabaseName("IX_Executions_BrokerExecutionId")
            .IsUnique()
            .HasFilter("BrokerExecutionId IS NOT NULL");

        // Ignore calculated properties
        builder.Ignore(e => e.Value);
    }
}
