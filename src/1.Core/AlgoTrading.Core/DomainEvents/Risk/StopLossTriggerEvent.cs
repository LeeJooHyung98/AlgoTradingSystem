using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.DomainEvents.Risk;

/// <summary>
/// description : 손절매 조건이 충족되어 실행되었을 때 발생하는 도메인 이벤트
/// Details : Risk Management Context에서 포지션의 가격이 손절 레벨에 도달하여 자동 청산이 트리거될 때 발행됩니다. StopLossType(Percentage, Fixed, ATR, Support, TrailingStop), TriggerPrice(실행가), LossAmount(손실 금액)를 포함하여 손절 상세 정보를 제공합니다. 이벤트 구독자는 즉시 시장가 매도 주문을 생성하여 포지션을 청산하고, 손절 실행 내역을 로그에 기록하며, 사용자에게 긴급 알림(Critical Severity)을 전송합니다. TrailingStop의 경우 수익이 발생한 포지션의 이익을 보호하므로 손실이 아닌 수익 확정으로 작용할 수도 있습니다.
/// Applied technology patterns : Domain Event Pattern, Risk Protection Event, Automatic Execution Pattern
/// </summary>
public class StopLossTriggerEvent : DomainEvent
{
    /// <summary>
    /// 포지션 ID
    /// </summary>
    public Guid PositionId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; }

    /// <summary>
    /// 손절매 유형
    /// </summary>
    public StopLossType StopLossType { get; }

    /// <summary>
    /// 실행 가격
    /// </summary>
    public decimal TriggerPrice { get; }

    /// <summary>
    /// 손실 금액
    /// </summary>
    public decimal LossAmount { get; }

    public StopLossTriggerEvent(Guid positionId, string accountNumber, string stockCode, StopLossType stopLossType, decimal triggerPrice, decimal lossAmount)
    {
        PositionId = positionId;
        AccountNumber = accountNumber;
        StockCode = stockCode;
        StopLossType = stopLossType;
        TriggerPrice = triggerPrice;
        LossAmount = lossAmount;
    }
}
