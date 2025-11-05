namespace AlgoTrading.Core.Exceptions.Risk;

/// <summary>
/// description : 리스크 한도를 초과했을 때 발생하는 예외
/// Details : Risk Management Context에서 RiskProfile에 정의된 한도(MaxPositionSize, MaxDailyLoss, MaxDrawdown 등)를 초과하는 거래를 시도할 때 발생합니다.
///           리스크 한도 위반은 자동으로 주문을 차단하며, Critical Severity 알림을 사용자에게 전송합니다.
///           이 예외는 금융 리스크 관리의 핵심 규칙을 강제하며, 과도한 손실로부터 계좌를 보호하는 안전 장치 역할을 합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Risk Management Pattern, Circuit Breaker Pattern
/// </summary>
public class RiskLimitExceededException : DomainException
{
    public string LimitType { get; }
    public decimal LimitValue { get; }
    public decimal CurrentValue { get; }

    public RiskLimitExceededException(string limitType, decimal limitValue, decimal currentValue)
        : base($"리스크 한도를 초과했습니다. LimitType: {limitType}, Limit: {limitValue:N2}, Current: {currentValue:N2}",
            errorCode: "RISK_LIMIT_EXCEEDED")
    {
        LimitType = limitType;
        LimitValue = limitValue;
        CurrentValue = currentValue;
    }

    public RiskLimitExceededException(Guid riskProfileId, string limitType, decimal limitValue, decimal currentValue)
        : base($"리스크 한도를 초과했습니다. RiskProfileId: {riskProfileId}, LimitType: {limitType}, Limit: {limitValue:N2}, Current: {currentValue:N2}",
            riskProfileId, "RISK_LIMIT_EXCEEDED")
    {
        LimitType = limitType;
        LimitValue = limitValue;
        CurrentValue = currentValue;
    }
}
