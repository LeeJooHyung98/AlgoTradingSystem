namespace AlgoTrading.Core.Exceptions.Strategy;

/// <summary>
/// description : 요청한 전략 ID를 찾을 수 없을 때 발생하는 예외
/// Details : Strategy Context에서 존재하지 않는 전략을 조회, 수정, 삭제하려고 할 때 발행됩니다.
///           잘못된 StrategyId가 제공되거나 전략이 삭제된 후 참조되는 경우에 발생하며, 클라이언트에게 404 Not Found 응답을 반환하는 데 사용됩니다.
///           이 예외는 데이터베이스 조회 실패가 아닌 도메인 규칙 위반(존재해야 할 전략이 없음)을 나타냅니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Pattern, Not Found Pattern
/// </summary>
public class StrategyNotFoundException : DomainException
{
    public StrategyNotFoundException(Guid strategyId)
        : base($"전략을 찾을 수 없습니다. StrategyId: {strategyId}", strategyId, "STRATEGY_NOT_FOUND")
    {
    }

    public StrategyNotFoundException(string strategyName)
        : base($"전략을 찾을 수 없습니다. StrategyName: {strategyName}", errorCode: "STRATEGY_NOT_FOUND")
    {
    }
}
