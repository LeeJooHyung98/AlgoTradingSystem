using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// description(설명) : TradingStrategy Entity Configuration (트레이딩 전략 엔티티 설정)
/// Details(상세설명) : EF Core configuration for TradingStrategy aggregate root mapping (TradingStrategy 집합 루트 매핑을 위한 EF Core 설정)
/// Applied technology patterns(적용기술패턴) : Fluent API Configuration, Aggregate Root Configuration Pattern (Fluent API 설정, 집합 루트 설정 패턴)
/// </summary>
public sealed class TradingStrategyConfiguration : IEntityTypeConfiguration<TradingStrategy>
{
    public void Configure(EntityTypeBuilder<TradingStrategy> builder)
    {
        // Table name and schema
        builder.ToTable("TradingStrategies", "strategy");

        // Primary key
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => new StrategyId(value))
            .IsRequired();

        // Basic properties
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<int>()
            .IsRequired();

        // Target stocks collection (JSON serialization)
        builder.Property(s => s.TargetStocks)
            .HasConversion(
                v => JsonSerializer.Serialize(v.Select(sc => sc.Value).ToList(), (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)!
                    .Select(val => new StockCode(val)).ToList())
            .HasColumnType("jsonb")
            .HasDefaultValue(new List<StockCode>());

        // Parameters (JSON serialization)
        builder.Property(s => s.Parameters)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null)!)
            .HasColumnType("jsonb")
            .HasDefaultValue(new Dictionary<string, object>());

        // Risk management properties
        builder.Property(s => s.MaxPositionSizePercent)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(s => s.StopLossPercent)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(s => s.TakeProfitPercent)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(10);

        // Performance metrics
        builder.Property(s => s.TotalTrades)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.WinningTrades)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.LosingTrades)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.TotalProfitLoss)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        // Trading hours
        builder.Property(s => s.TradingStartTime)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString(@"hh\:mm\:ss") : null,
                v => !string.IsNullOrEmpty(v) ? TimeSpan.Parse(v) : (TimeSpan?)null);

        builder.Property(s => s.TradingEndTime)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString(@"hh\:mm\:ss") : null,
                v => !string.IsNullOrEmpty(v) ? TimeSpan.Parse(v) : (TimeSpan?)null);

        // Audit properties
        builder.Property(s => s.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.LastActivatedAt);

        builder.Property(s => s.LastDeactivatedAt);

        // Owned entities - Rules collection
        builder.OwnsMany<StrategyRule>(s => s.Rules, rules =>
        {
            rules.ToTable("StrategyRules", "strategy");

            // Configure Id property with value object conversion
            rules.Property(r => r.Id)
                .HasConversion(
                    id => id.Value,
                    value => new StrategyRuleId(value))
                .IsRequired()
                .ValueGeneratedNever();

            rules.HasKey(r => r.Id);

            // Configure audit properties from Entity base class
            rules.Property(r => r.CreatedAt)
                .IsRequired();

            rules.Property(r => r.LastModifiedAt);

            rules.Property(r => r.Name)
                .HasMaxLength(200)
                .IsRequired();

            rules.Property(r => r.Description)
                .HasMaxLength(2000);

            rules.Property(r => r.Type)
                .HasConversion<int>()
                .IsRequired();

            rules.Property(r => r.Condition)
                .HasMaxLength(4000)
                .IsRequired();

            rules.Property(r => r.Priority)
                .IsRequired()
                .HasDefaultValue(0);

            rules.Property(r => r.IsEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            rules.Property(r => r.Parameters)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null)!)
                .HasColumnType("jsonb")
                .HasDefaultValue(new Dictionary<string, object>());

            rules.Property(r => r.Timeframe)
                .HasMaxLength(20);

            rules.HasIndex(r => r.Type)
                .HasDatabaseName("IX_StrategyRules_Type");
        });

        // Owned entities - Signals collection
        builder.OwnsMany<StrategySignal>(s => s.Signals, signals =>
        {
            signals.ToTable("StrategySignals", "strategy");

            // Configure Id property with value object conversion
            signals.Property(sig => sig.Id)
                .HasConversion(
                    id => id.Value,
                    value => new StrategySignalId(value))
                .IsRequired()
                .ValueGeneratedNever();

            signals.HasKey(sig => sig.Id);

            // Configure audit properties from Entity base class
            signals.Property(sig => sig.CreatedAt)
                .IsRequired();

            signals.Property(sig => sig.LastModifiedAt);

            // Configure StrategyId foreign key
            signals.Property(sig => sig.StrategyId)
                .HasConversion(
                    id => id.Value,
                    value => new StrategyId(value))
                .IsRequired();

            signals.Property(sig => sig.StockCode)
                .HasConversion(
                    sc => sc.Value,
                    value => new StockCode(value))
                .HasMaxLength(6)
                .IsRequired();

            signals.Property(sig => sig.SignalType)
                .HasConversion<int>()
                .IsRequired();

            signals.Property(sig => sig.Strength)
                .HasPrecision(5, 2)
                .IsRequired();

            signals.Property(sig => sig.Confidence)
                .HasPrecision(5, 4)
                .IsRequired();

            signals.Property(sig => sig.SuggestedPrice)
                .HasPrecision(18, 2);

            signals.Property(sig => sig.SuggestedQuantity);

            signals.Property(sig => sig.StopLossPrice)
                .HasPrecision(18, 2);

            signals.Property(sig => sig.TakeProfitPrice)
                .HasPrecision(18, 2);

            signals.Property(sig => sig.GeneratedAt)
                .IsRequired();

            signals.Property(sig => sig.ExpiresAt);

            signals.Property(sig => sig.Status)
                .HasConversion<int>()
                .IsRequired();

            signals.Property(sig => sig.WasActedUpon)
                .IsRequired()
                .HasDefaultValue(false);

            signals.Property(sig => sig.OrderId);

            signals.Property(sig => sig.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null)!)
                .HasColumnType("jsonb")
                .HasDefaultValue(new Dictionary<string, object>());

            signals.Property(sig => sig.Reason)
                .HasMaxLength(1000);

            signals.HasIndex(sig => sig.StockCode)
                .HasDatabaseName("IX_StrategySignals_StockCode");

            signals.HasIndex(sig => sig.GeneratedAt)
                .HasDatabaseName("IX_StrategySignals_GeneratedAt");

            signals.HasIndex(sig => sig.Status)
                .HasDatabaseName("IX_StrategySignals_Status");
        });

        // Indexes for query optimization
        builder.HasIndex(s => s.Name)
            .HasDatabaseName("IX_TradingStrategies_Name");

        builder.HasIndex(s => s.Type)
            .HasDatabaseName("IX_TradingStrategies_Type");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_TradingStrategies_Status");

        builder.HasIndex(s => s.CreatedBy)
            .HasDatabaseName("IX_TradingStrategies_CreatedBy");

        builder.HasIndex(s => new { s.Status, s.Type })
            .HasDatabaseName("IX_TradingStrategies_Status_Type");

        // Ignore computed properties
        builder.Ignore(s => s.WinRate);

        // Ignore navigation properties (domain events)
        builder.Ignore(s => s.DomainEvents);
    }
}
