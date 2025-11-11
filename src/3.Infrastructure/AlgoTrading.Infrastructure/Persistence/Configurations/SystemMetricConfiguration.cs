using AlgoTrading.Core.Entities.Monitoring;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlgoTrading.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for SystemMetric entity
/// Maps to monitoring.system_metrics table (TimescaleDB Hypertable)
/// </summary>
public class SystemMetricConfiguration : IEntityTypeConfiguration<SystemMetric>
{
    public void Configure(EntityTypeBuilder<SystemMetric> builder)
    {
        // Table configuration - TimescaleDB Hypertable
        builder.ToTable("system_metrics", "monitoring");

        // Primary key (metric_timestamp)
        builder.HasKey(s => s.MetricTimestamp);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new SystemMetricId(value))
            .IsRequired();

        builder.Property(s => s.MetricTimestamp)
            .HasColumnName("metric_timestamp")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(s => s.CpuUsagePercent)
            .HasColumnName("cpu_usage_percent")
            .HasColumnType("decimal(5,2)");

        builder.Property(s => s.MemoryUsagePercent)
            .HasColumnName("memory_usage_percent")
            .HasColumnType("decimal(5,2)");

        builder.Property(s => s.MemoryUsedMb)
            .HasColumnName("memory_used_mb");

        builder.Property(s => s.MemoryTotalMb)
            .HasColumnName("memory_total_mb");

        builder.Property(s => s.NetworkInMbps)
            .HasColumnName("network_in_mbps")
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.NetworkOutMbps)
            .HasColumnName("network_out_mbps")
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.DbConnections)
            .HasColumnName("db_connections");

        builder.Property(s => s.DbQueryTimeMs)
            .HasColumnName("db_query_time_ms")
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.ApiRequestCount)
            .HasColumnName("api_request_count");

        builder.Property(s => s.ApiErrorCount)
            .HasColumnName("api_error_count");

        builder.Property(s => s.ApiAvgLatencyMs)
            .HasColumnName("api_avg_latency_ms")
            .HasColumnType("decimal(10,2)");

        // Note: TimescaleDB Hypertable conversion is done via migration, not in configuration
    }
}
