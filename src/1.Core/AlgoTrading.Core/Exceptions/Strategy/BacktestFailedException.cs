namespace AlgoTrading.Core.Exceptions.Strategy;

/// <summary>
/// description : 백테스트 실행 중 오류가 발생하여 완료하지 못했을 때 발생하는 예외
/// Details : Backtesting Service에서 전략 백테스트 실행 도중 데이터 부족, 계산 오류, 메모리 초과 등의 문제로 백테스트를 완료할 수 없는 경우 발생합니다.
///           백테스트는 장시간 실행되는 작업이므로, 중간에 실패 시 사용자에게 명확한 실패 원인(FailureReason)과 함께 알림을 전송해야 합니다.
///           이 예외는 도메인 계층에서 발생하지만, 백테스트 서비스의 비즈니스 규칙(최소 데이터 요구사항, 메모리 한계)을 표현합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Pattern, Long-Running Process Pattern
/// </summary>
public class BacktestFailedException : DomainException
{
    public string FailureReason { get; }

    public BacktestFailedException(Guid strategyId, string failureReason)
        : base($"백테스트 실행에 실패했습니다. StrategyId: {strategyId}, Reason: {failureReason}", strategyId, "BACKTEST_FAILED")
    {
        FailureReason = failureReason;
    }

    public BacktestFailedException(Guid strategyId, string failureReason, Exception innerException)
        : base($"백테스트 실행에 실패했습니다. StrategyId: {strategyId}, Reason: {failureReason}", innerException, strategyId, "BACKTEST_FAILED")
    {
        FailureReason = failureReason;
    }
}
