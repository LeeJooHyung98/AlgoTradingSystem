namespace AlgoTrading.Core.Exceptions.Strategy;

/// <summary>
/// description : 이미 활성화된 전략을 다시 활성화하려고 할 때 발생하는 예외
/// Details : Strategy 엔티티의 불변성 규칙 중 하나로, 이미 Active 상태인 전략을 중복 활성화하려는 시도를 차단합니다.
///           State Machine 패턴에서 상태 전이 규칙을 위반한 경우에 해당하며, 전략이 실행 중일 때 발생 가능한 데이터 불일치를 방지합니다.
///           클라이언트는 이 예외를 받으면 현재 전략 상태를 다시 확인하고, 필요 시 비활성화 후 재활성화해야 합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), State Machine Pattern, Invariant Protection
/// </summary>
public class StrategyAlreadyActiveException : DomainException
{
    public StrategyAlreadyActiveException(Guid strategyId)
        : base($"전략이 이미 활성화되어 있습니다. StrategyId: {strategyId}", strategyId, "STRATEGY_ALREADY_ACTIVE")
    {
    }
}
