using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.DomainEvents.Strategy;

/// <summary>
/// description : 전략에 의해 매매 신호가 생성되었을 때 발생하는 도메인 이벤트
/// Details : 매수/매도/손절/익절 등의 신호가 생성되면 발행되며, 주문 생성, 알림 전송 등의 후속 작업을 트리거합니다.
///           신호 강도와 추천 가격 정보를 포함하여 의사결정을 지원합니다.
/// Applied technology patterns : Domain Event Pattern, Event-Driven Architecture, Strategy Pattern, Observer Pattern
/// </summary>
public class SignalGeneratedEvent : DomainEvent
{
    /// <summary>
    /// 신호 ID
    /// </summary>
    public Guid SignalId { get; }

    /// <summary>
    /// 전략 ID
    /// </summary>
    public Guid StrategyId { get; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; }

    /// <summary>
    /// 신호 유형
    /// </summary>
    public SignalType SignalType { get; }

    /// <summary>
    /// 신호 강도
    /// </summary>
    public decimal Strength { get; }

    /// <summary>
    /// 추천 가격
    /// </summary>
    public decimal? RecommendedPrice { get; }

    public SignalGeneratedEvent(Guid signalId, Guid strategyId, string stockCode, SignalType signalType, decimal strength, decimal? recommendedPrice = null)
    {
        SignalId = signalId;
        StrategyId = strategyId;
        StockCode = stockCode;
        SignalType = signalType;
        Strength = strength;
        RecommendedPrice = recommendedPrice;
    }
}
