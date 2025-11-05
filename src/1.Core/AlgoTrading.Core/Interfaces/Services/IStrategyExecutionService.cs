using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 트레이딩 전략의 실행을 담당하는 도메인 서비스 인터페이스
/// Details : 전략 활성화/비활성화, 신호 생성, 전략 실행 등 전략 생명주기 관리 기능을 제공합니다.
///           Application Layer의 Orchestration을 통해 여러 Aggregate를 조율합니다.
/// Applied technology patterns : Domain Service Pattern, Strategy Pattern, Command Pattern, Domain-Driven Design
/// </summary>
public interface IStrategyExecutionService
{
    /// <summary>
    /// 전략 실행
    /// </summary>
    Task ExecuteStrategyAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 활성화
    /// </summary>
    Task ActivateStrategyAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 비활성화
    /// </summary>
    Task DeactivateStrategyAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 신호 생성
    /// </summary>
    Task<StrategySignal> GenerateSignalAsync(Guid strategyId, string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 모든 활성화된 전략 실행
    /// </summary>
    Task ExecuteAllActiveStrategiesAsync(CancellationToken cancellationToken = default);
}
