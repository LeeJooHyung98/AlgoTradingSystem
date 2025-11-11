using AlgoTrading.Core.Entities.Monitoring;
using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Entities.Strategy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Alert entity
/// Maps to monitoring.alerts table
/// NOTE: There are property mismatches between entity and DB schema that need to be resolved
/// </summary>
public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        // Table configuration
        builder.ToTable("alerts", "monitoring");

        // Primary key
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new AlertId(value))
            .IsRequired();

        builder.Property(a => a.Type)
            .HasColumnName("alert_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("alert_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Severity)
            .HasColumnName("severity")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Message)
            .HasColumnName("message")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.AccountId)
            .HasColumnName("account_id")
            .HasConversion(
                id => id!.Value,
                value => new AccountId(value));

        builder.Property(a => a.StrategyId)
            .HasColumnName("strategy_id")
            .HasConversion(
                id => id!.Value,
                value => new StrategyId(value));

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(a => a.AcknowledgedAt)
            .HasColumnName("acknowledged_at")
            .HasColumnType("timestamptz");

        builder.Property(a => a.ResolvedAt)
            .HasColumnName("resolved_at")
            .HasColumnType("timestamptz");

        // Properties not in DB schema - need to be ignored or added to DB
        builder.Ignore(a => a.Source);
        builder.Ignore(a => a.Channel);
        builder.Ignore(a => a.RecipientTarget);
        builder.Ignore(a => a.OccurrenceCount);
        builder.Ignore(a => a.TriggeredAt);
        builder.Ignore(a => a.SentAt);
        builder.Ignore(a => a.AcknowledgedBy);
        builder.Ignore(a => a.ResolutionNotes);
        builder.Ignore(a => a.NextAllowedAlertTime);

        // Indexes
        builder.HasIndex(a => a.Status)
            .HasDatabaseName("idx_alerts_status");

        builder.HasIndex(a => a.Severity)
            .HasDatabaseName("idx_alerts_severity");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("idx_alerts_created")
            .IsDescending();

        builder.HasIndex(a => a.AccountId)
            .HasDatabaseName("idx_alerts_account");

        // Note: DB has order_id column and metadata JSONB column that don't exist in entity
        // These should be added to the entity or removed from the DB schema
        // Foreign keys will be added in migration
    }
}
