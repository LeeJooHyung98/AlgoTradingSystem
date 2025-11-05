namespace AlgoTrading.Core.Exceptions.Risk;

/// <summary>
/// description : 리스크 프로필 설정이 유효하지 않을 때 발생하는 예외
/// Details : RiskProfile 엔티티 생성 또는 수정 시, 리스크 파라미터가 도메인 규칙을 위반하는 경우 발생합니다.
///           예: MaxPositionSize가 0 이하, MaxDailyLoss가 100% 초과, StopLoss가 TakeProfit보다 큼 등의 비즈니스 규칙 위반.
///           리스크 프로필은 계좌의 모든 거래에 영향을 미치므로, 잘못된 설정은 심각한 손실로 이어질 수 있어 엄격하게 검증됩니다.
///           이 예외는 리스크 관리 시스템의 불변성을 보호하는 데 사용됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Validation Pattern, Invariant Protection
/// </summary>
public class InvalidRiskProfileException : DomainException
{
    public string ValidationError { get; }

    public InvalidRiskProfileException(string validationError)
        : base($"리스크 프로필 설정이 유효하지 않습니다. ValidationError: {validationError}", errorCode: "INVALID_RISK_PROFILE")
    {
        ValidationError = validationError;
    }

    public InvalidRiskProfileException(Guid riskProfileId, string validationError)
        : base($"리스크 프로필 설정이 유효하지 않습니다. RiskProfileId: {riskProfileId}, ValidationError: {validationError}",
            riskProfileId, "INVALID_RISK_PROFILE")
    {
        ValidationError = validationError;
    }
}
