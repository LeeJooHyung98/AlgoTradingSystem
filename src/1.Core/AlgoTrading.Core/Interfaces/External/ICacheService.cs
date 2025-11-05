namespace AlgoTrading.Core.Interfaces.External;

/// <summary>
/// description : 분산 캐시 및 인메모리 캐시 관리 서비스 인터페이스
/// Details : Infrastructure Layer에서 L1(MemoryCache) 및 L2(Redis) 캐싱을 추상화한 인터페이스입니다. 자주 조회되는 데이터(종목 메타데이터, 포트폴리오 스냅샷, 기술적 지표 계산 결과)를 캐싱하여 데이터베이스 부하를 줄이고 응답 속도를 개선합니다. TTL(Time-To-Live)을 설정하여 데이터 신선도를 유지하고, 패턴 기반 삭제로 관련 캐시를 일괄 무효화합니다. Domain Event Handler에서 엔티티 변경 시 해당 캐시를 삭제하여 일관성을 보장하며, 분산 환경에서 Redis를 통해 서버 간 캐시를 공유합니다.
/// Applied technology patterns : Cache-Aside Pattern, Distributed Cache Pattern, Cache Invalidation Pattern
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 캐시에서 값 조회
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 캐시에 값 저장
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 캐시에서 값 삭제
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 캐시 존재 여부 확인
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 패턴으로 캐시 키 삭제
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// 캐시 만료 시간 설정
    /// </summary>
    Task SetExpirationAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// 여러 값 조회
    /// </summary>
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// 여러 값 저장
    /// </summary>
    Task SetManyAsync<T>(IDictionary<string, T> values, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}
