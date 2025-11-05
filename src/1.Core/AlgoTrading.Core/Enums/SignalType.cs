namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 트레이딩 전략이 생성하는 매매 신호의 유형
/// Details : 알고리즘 트레이딩 전략이 시장 분석 결과로 생성하는 신호를 Buy(매수), Sell(매도), StopLoss(손절매), TakeProfit(익절매), Hold(보유), Close(청산)로 분류합니다. Strategy Context에서 SignalGeneratedEvent를 발행할 때 신호 유형을 지정하며, Trading Context의 Order Management Service가 신호 유형에 따라 적절한 주문 타입(시장가/지정가)과 수량을 결정합니다. StopLoss와 TakeProfit 신호는 Risk Management Context와 연동되어 포지션 보호 로직을 수행합니다.
/// Applied technology patterns : Command Pattern, Event-Driven Architecture, Domain Event Pattern
/// </summary>
public enum SignalType
{
    /// <summary>
    /// 매수 신호
    /// </summary>
    Buy = 1,

    /// <summary>
    /// 매도 신호
    /// </summary>
    Sell = 2,

    /// <summary>
    /// 손절매 신호
    /// </summary>
    StopLoss = 3,

    /// <summary>
    /// 익절매 신호
    /// </summary>
    TakeProfit = 4,

    /// <summary>
    /// 보유 신호
    /// </summary>
    Hold = 5,

    /// <summary>
    /// 청산 신호
    /// </summary>
    Close = 6
}
