using AlgoTrading.Core.Entities.Audit;
using AlgoTrading.Core.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AuditLog entity
/// Maps to audit.audit_logs table (TimescaleDB Hypertable)
/// NOTE: There are property mismatches between entity and DB schema that need to be resolved
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Table configuration - TimescaleDB Hypertable
        builder.ToTable("audit_logs", "audit");

        // Composite primary key (log_timestamp, id)
        builder.HasKey(a => new { a.LogTimestamp, a.Id });

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new AuditLogId(value))
            .IsRequired();

        builder.Property(a => a.LogTimestamp)
            .HasColumnName("log_timestamp")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(a => a.Action)
            .HasColumnName("action_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .IsRequired();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(a => a.UserIp)
            .HasColumnName("user_ip")
            .HasMaxLength(45);

        // JSON properties - stored as JSONB in database
        builder.Property(a => a.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // No properties to ignore - all entity properties match DB schema

        // Indexes
        builder.HasIndex(a => new { a.UserId, a.LogTimestamp })
            .HasDatabaseName("idx_audit_user")
            .IsDescending(false, true); // log_timestamp DESC

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("idx_audit_entity");

        // Note: TimescaleDB Hypertable conversion is done via migration, not in configuration
        // Foreign keys will be added in migration
    }
}
