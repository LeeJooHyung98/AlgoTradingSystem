using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Monitoring;

/// <summary>
/// description(설명) : System Metric entity for system performance monitoring (시스템 성능 모니터링을 위한 시스템 메트릭 엔티티)
/// Details(상세설명) : Time-series data with CPU, memory, network, database, and API metrics stored in TimescaleDB (TimescaleDB에 저장되는 CPU, 메모리, 네트워크, 데이터베이스, API 지표를 포함한 시계열 데이터)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Time-Series Pattern, Monitoring Pattern (엔티티 패턴, 시계열 패턴, 모니터링 패턴)
/// </summary>
public sealed class SystemMetric : Entity<SystemMetricId>
{
    /// <summary>
    /// 메트릭 수집 시각 (TimescaleDB 시계열 기준)
    /// </summary>
    public DateTime MetricTimestamp { get; private set; }

    /// <summary>
    /// CPU 사용률 (%)
    /// </summary>
    public decimal? CpuUsagePercent { get; private set; }

    /// <summary>
    /// 메모리 사용률 (%)
    /// </summary>
    public decimal? MemoryUsagePercent { get; private set; }

    /// <summary>
    /// 사용중인 메모리 (MB)
    /// </summary>
    public long? MemoryUsedMb { get; private set; }

    /// <summary>
    /// 전체 메모리 (MB)
    /// </summary>
    public long? MemoryTotalMb { get; private set; }

    /// <summary>
    /// 네트워크 수신 속도 (Mbps)
    /// </summary>
    public decimal? NetworkInMbps { get; private set; }

    /// <summary>
    /// 네트워크 송신 속도 (Mbps)
    /// </summary>
    public decimal? NetworkOutMbps { get; private set; }

    /// <summary>
    /// 데이터베이스 연결 수
    /// </summary>
    public int? DbConnections { get; private set; }

    /// <summary>
    /// 평균 쿼리 실행 시간 (ms)
    /// </summary>
    public decimal? DbQueryTimeMs { get; private set; }

    /// <summary>
    /// API 요청 수
    /// </summary>
    public int? ApiRequestCount { get; private set; }

    /// <summary>
    /// API 오류 수
    /// </summary>
    public int? ApiErrorCount { get; private set; }

    /// <summary>
    /// API 평균 응답 시간 (ms)
    /// </summary>
    public decimal? ApiAvgLatencyMs { get; private set; }

    private SystemMetric() { }

    private SystemMetric(
        SystemMetricId id,
        DateTime metricTimestamp)
    {
        Id = id;
        MetricTimestamp = metricTimestamp;
    }

    /// <summary>
    /// description(설명) : Create system metric record (시스템 메트릭 레코드 생성)
    /// Details(상세설명) : Factory method to create system monitoring snapshot (시스템 모니터링 스냅샷을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static SystemMetric Create(DateTime metricTimestamp)
    {
        var id = SystemMetricId.New();
        return new SystemMetric(id, metricTimestamp);
    }

    /// <summary>
    /// description(설명) : Update CPU and memory metrics (CPU 및 메모리 지표 업데이트)
    /// Details(상세설명) : Updates CPU and memory usage metrics (CPU 및 메모리 사용 지표 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateCpuMemoryMetrics(
        decimal? cpuUsagePercent,
        decimal? memoryUsagePercent,
        long? memoryUsedMb,
        long? memoryTotalMb)
    {
        CpuUsagePercent = cpuUsagePercent;
        MemoryUsagePercent = memoryUsagePercent;
        MemoryUsedMb = memoryUsedMb;
        MemoryTotalMb = memoryTotalMb;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update network metrics (네트워크 지표 업데이트)
    /// Details(상세설명) : Updates network in/out bandwidth (네트워크 수신/송신 대역폭 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateNetworkMetrics(decimal? networkInMbps, decimal? networkOutMbps)
    {
        NetworkInMbps = networkInMbps;
        NetworkOutMbps = networkOutMbps;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update database metrics (데이터베이스 지표 업데이트)
    /// Details(상세설명) : Updates database connections and query time (데이터베이스 연결 수 및 쿼리 시간 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateDatabaseMetrics(int? dbConnections, decimal? dbQueryTimeMs)
    {
        DbConnections = dbConnections;
        DbQueryTimeMs = dbQueryTimeMs;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update API metrics (API 지표 업데이트)
    /// Details(상세설명) : Updates API request count, error count, and latency (API 요청 수, 오류 수, 응답 시간 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateApiMetrics(int? apiRequestCount, int? apiErrorCount, decimal? apiAvgLatencyMs)
    {
        ApiRequestCount = apiRequestCount;
        ApiErrorCount = apiErrorCount;
        ApiAvgLatencyMs = apiAvgLatencyMs;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if system is healthy (시스템이 정상인지 확인)
    /// Details(상세설명) : Returns true if CPU < 80%, Memory < 80%, API error rate < 5% (CPU < 80%, 메모리 < 80%, API 오류율 < 5%이면 true 반환)
    /// </summary>
    public bool IsHealthy()
    {
        // Check CPU and Memory
        if (CpuUsagePercent.HasValue && CpuUsagePercent.Value > 80)
            return false;

        if (MemoryUsagePercent.HasValue && MemoryUsagePercent.Value > 80)
            return false;

        // Check API error rate
        if (ApiRequestCount.HasValue && ApiErrorCount.HasValue && ApiRequestCount.Value > 0)
        {
            var errorRate = (decimal)ApiErrorCount.Value / ApiRequestCount.Value * 100;
            if (errorRate > 5)
                return false;
        }

        return true;
    }
}

/// <summary>
/// description(설명) : System Metric ID value object (시스템 메트릭 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for SystemMetric (SystemMetric의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class SystemMetricId : GuidId
{
    public SystemMetricId(Guid value) : base(value) { }

    public static SystemMetricId New() => new(Guid.NewGuid());
}
