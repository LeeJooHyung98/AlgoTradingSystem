namespace AlgoTrading.Core.Exceptions.Strategy;

/// <summary>
/// description : 전략 파라미터가 유효하지 않을 때 발생하는 예외
/// Details : Strategy 엔티티 생성 또는 파라미터 업데이트 시, 파라미터 값이 도메인 규칙을 위반하는 경우 발생합니다.
///           예: MovingAverage 기간이 음수, RSI 임계값이 0-100 범위 밖, StopLoss 비율이 100% 초과 등의 비즈니스 규칙 위반.
///           FluentValidation으로 검증할 수 없는 복잡한 도메인 불변성(여러 파라미터 간 상호 의존성)을 보호하는 데 사용됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Parameter Validation Pattern, Invariant Protection
/// </summary>
public class InvalidStrategyParameterException : DomainException
{
    public string ParameterName { get; }

    public InvalidStrategyParameterException(string parameterName, string reason)
        : base($"전략 파라미터가 유효하지 않습니다. Parameter: {parameterName}, Reason: {reason}", errorCode: "INVALID_STRATEGY_PARAMETER")
    {
        ParameterName = parameterName;
    }

    public InvalidStrategyParameterException(Guid strategyId, string parameterName, string reason)
        : base($"전략 파라미터가 유효하지 않습니다. StrategyId: {strategyId}, Parameter: {parameterName}, Reason: {reason}",
            strategyId, "INVALID_STRATEGY_PARAMETER")
    {
        ParameterName = parameterName;
    }
}
