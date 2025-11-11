using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlgoTrading.IntegrationTests.TestHelpers;

/// <summary>
/// Test-specific DbContext for Strategy Integration Tests
/// Only configures TradingStrategy and related entities to avoid configuration issues with other bounded contexts
/// </summary>
public class StrategyTestDbContext : DbContext
{
    public DbSet<TradingStrategy> TradingStrategies { get; set; } = null!;

    public StrategyTestDbContext(DbContextOptions<StrategyTestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TradingStrategy entity
        var builder = modelBuilder.Entity<TradingStrategy>();

        builder.ToTable("TradingStrategies", "strategy");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => new StrategyId(value))
            .IsRequired();

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

        builder.Property(s => s.TargetStocks)
            .HasConversion(
                v => JsonSerializer.Serialize(v.Select(sc => sc.Value).ToList(), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!
                    .Select(val => new StockCode(val)).ToList())
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue(new List<StockCode>());

        builder.Property(s => s.Parameters)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue(new Dictionary<string, object>());

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

        builder.Property(s => s.TradingStartTime);
        builder.Property(s => s.TradingEndTime);

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.LastActivatedAt);
        builder.Property(s => s.LastDeactivatedAt);

        // Owned entities - Rules collection
        builder.OwnsMany<StrategyRule>(s => s.Rules, rules =>
        {
            rules.ToTable("StrategyRules", "strategy");

            rules.Property(r => r.Id)
                .HasConversion(
                    id => id.Value,
                    value => new StrategyRuleId(value))
                .IsRequired()
                .ValueGeneratedNever();

            rules.HasKey(r => r.Id);

            rules.Property(r => r.CreatedAt).IsRequired();
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
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
                .HasColumnType("nvarchar(max)")
                .HasDefaultValue(new Dictionary<string, object>());

            rules.Property(r => r.Timeframe)
                .HasMaxLength(20);
        });

        // Owned entities - Signals collection
        builder.OwnsMany<StrategySignal>(s => s.Signals, signals =>
        {
            signals.ToTable("StrategySignals", "strategy");

            signals.Property(sig => sig.Id)
                .HasConversion(
                    id => id.Value,
                    value => new StrategySignalId(value))
                .IsRequired()
                .ValueGeneratedNever();

            signals.HasKey(sig => sig.Id);

            signals.Property(sig => sig.CreatedAt).IsRequired();
            signals.Property(sig => sig.LastModifiedAt);

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
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)!)
                .HasColumnType("nvarchar(max)")
                .HasDefaultValue(new Dictionary<string, object>());

            signals.Property(sig => sig.Reason)
                .HasMaxLength(1000);
        });

        // Ignore computed properties
        builder.Ignore(s => s.WinRate);
        builder.Ignore(s => s.DomainEvents);
    }
}
