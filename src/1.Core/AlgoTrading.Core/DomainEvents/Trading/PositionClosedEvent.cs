namespace AlgoTrading.Core.DomainEvents.Trading;

/// <summary>
/// description : 포지션이 완전히 청산되었을 때 발생하는 도메인 이벤트
/// Details : Trading Context에서 매도 주문 체결로 포지션의 보유 수량이 0이 되어 Position 엔티티가 Closed 상태로 전환될 때 발행됩니다. ExitPrice(청산가), RealizedPL(실현 손익), ReturnPercentage(수익률 %)를 포함하여 거래 결과를 전달합니다. 이벤트 구독자는 Trade 레코드를 생성하여 거래 내역을 영구 저장하고, 포트폴리오의 현금 잔고를 업데이트하며, 성과 지표(승률, Sharpe Ratio)를 재계산하고, 사용자에게 청산 결과를 알립니다. 수익/손실에 따라 전략의 파라미터 조정이나 리스크 프로필 변경이 트리거될 수 있습니다.
/// Applied technology patterns : Domain Event Pattern, Trade Lifecycle Pattern, Performance Tracking Pattern
/// </summary>
public class PositionClosedEvent : DomainEvent
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
    /// 청산 수량
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// 청산 가격
    /// </summary>
    public decimal ExitPrice { get; }

    /// <summary>
    /// 실현 손익
    /// </summary>
    public decimal RealizedPL { get; }

    /// <summary>
    /// 수익률 (%)
    /// </summary>
    public decimal ReturnPercentage { get; }

    public PositionClosedEvent(Guid positionId, string accountNumber, string stockCode, int quantity, decimal exitPrice, decimal realizedPL, decimal returnPercentage)
    {
        PositionId = positionId;
        AccountNumber = accountNumber;
        StockCode = stockCode;
        Quantity = quantity;
        ExitPrice = exitPrice;
        RealizedPL = realizedPL;
        ReturnPercentage = returnPercentage;
    }
}
