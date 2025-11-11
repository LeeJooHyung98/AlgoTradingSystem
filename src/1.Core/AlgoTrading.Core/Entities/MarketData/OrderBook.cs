using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description(설명) : Order Book entity for 10-level bid/ask quote data (10단계 호가 데이터를 위한 호가창 엔티티)
/// Details(상세설명) : Real-time order book with 10 levels of bid and ask prices and volumes (10단계 매수/매도 호가와 잔량을 포함한 실시간 호가창)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Value Object Pattern (엔티티 패턴, 값 객체 패턴)
/// </summary>
public sealed class OrderBook : Entity<OrderBookId>
{
    /// <summary>
    /// 종목 코드
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// 호가 캡처 시각
    /// </summary>
    public DateTime CapturedAt { get; private set; }

    /// <summary>
    /// 매도 호가 1단계 (가장 낮은 매도 가격)
    /// </summary>
    public Money? AskPrice1 { get; private set; }
    public long? AskVolume1 { get; private set; }

    /// <summary>
    /// 매도 호가 2단계
    /// </summary>
    public Money? AskPrice2 { get; private set; }
    public long? AskVolume2 { get; private set; }

    /// <summary>
    /// 매도 호가 3단계
    /// </summary>
    public Money? AskPrice3 { get; private set; }
    public long? AskVolume3 { get; private set; }

    /// <summary>
    /// 매도 호가 4단계
    /// </summary>
    public Money? AskPrice4 { get; private set; }
    public long? AskVolume4 { get; private set; }

    /// <summary>
    /// 매도 호가 5단계
    /// </summary>
    public Money? AskPrice5 { get; private set; }
    public long? AskVolume5 { get; private set; }

    /// <summary>
    /// 매도 호가 6단계
    /// </summary>
    public Money? AskPrice6 { get; private set; }
    public long? AskVolume6 { get; private set; }

    /// <summary>
    /// 매도 호가 7단계
    /// </summary>
    public Money? AskPrice7 { get; private set; }
    public long? AskVolume7 { get; private set; }

    /// <summary>
    /// 매도 호가 8단계
    /// </summary>
    public Money? AskPrice8 { get; private set; }
    public long? AskVolume8 { get; private set; }

    /// <summary>
    /// 매도 호가 9단계
    /// </summary>
    public Money? AskPrice9 { get; private set; }
    public long? AskVolume9 { get; private set; }

    /// <summary>
    /// 매도 호가 10단계
    /// </summary>
    public Money? AskPrice10 { get; private set; }
    public long? AskVolume10 { get; private set; }

    /// <summary>
    /// 매수 호가 1단계 (가장 높은 매수 가격)
    /// </summary>
    public Money? BidPrice1 { get; private set; }
    public long? BidVolume1 { get; private set; }

    /// <summary>
    /// 매수 호가 2단계
    /// </summary>
    public Money? BidPrice2 { get; private set; }
    public long? BidVolume2 { get; private set; }

    /// <summary>
    /// 매수 호가 3단계
    /// </summary>
    public Money? BidPrice3 { get; private set; }
    public long? BidVolume3 { get; private set; }

    /// <summary>
    /// 매수 호가 4단계
    /// </summary>
    public Money? BidPrice4 { get; private set; }
    public long? BidVolume4 { get; private set; }

    /// <summary>
    /// 매수 호가 5단계
    /// </summary>
    public Money? BidPrice5 { get; private set; }
    public long? BidVolume5 { get; private set; }

    /// <summary>
    /// 매수 호가 6단계
    /// </summary>
    public Money? BidPrice6 { get; private set; }
    public long? BidVolume6 { get; private set; }

    /// <summary>
    /// 매수 호가 7단계
    /// </summary>
    public Money? BidPrice7 { get; private set; }
    public long? BidVolume7 { get; private set; }

    /// <summary>
    /// 매수 호가 8단계
    /// </summary>
    public Money? BidPrice8 { get; private set; }
    public long? BidVolume8 { get; private set; }

    /// <summary>
    /// 매수 호가 9단계
    /// </summary>
    public Money? BidPrice9 { get; private set; }
    public long? BidVolume9 { get; private set; }

    /// <summary>
    /// 매수 호가 10단계
    /// </summary>
    public Money? BidPrice10 { get; private set; }
    public long? BidVolume10 { get; private set; }

    /// <summary>
    /// 총 매도 호가 잔량 (10단계 합계)
    /// </summary>
    public long? TotalAskVolume { get; private set; }

    /// <summary>
    /// 총 매수 호가 잔량 (10단계 합계)
    /// </summary>
    public long? TotalBidVolume { get; private set; }

    private OrderBook() { }

    private OrderBook(
        OrderBookId id,
        StockCode stockCode,
        DateTime capturedAt)
    {
        Id = id;
        StockCode = stockCode;
        CapturedAt = capturedAt;
    }

    /// <summary>
    /// description(설명) : Create order book snapshot (호가창 스냅샷 생성)
    /// Details(상세설명) : Factory method to create order book (호가창을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static OrderBook Create(
        StockCode stockCode,
        DateTime capturedAt)
    {
        var id = OrderBookId.New();
        return new OrderBook(id, stockCode, capturedAt);
    }

    /// <summary>
    /// description(설명) : Update all price levels (모든 호가 단계 업데이트)
    /// Details(상세설명) : Updates all 10 levels for bid and ask sides (매수/매도 10단계 호가 전체 업데이트)
    /// Applied technology patterns(적용기술패턴) : Builder Pattern (빌더 패턴)
    /// </summary>
    public void UpdateLevels(
        Money? askPrice1, long? askVolume1,
        Money? askPrice2, long? askVolume2,
        Money? askPrice3, long? askVolume3,
        Money? askPrice4, long? askVolume4,
        Money? askPrice5, long? askVolume5,
        Money? askPrice6, long? askVolume6,
        Money? askPrice7, long? askVolume7,
        Money? askPrice8, long? askVolume8,
        Money? askPrice9, long? askVolume9,
        Money? askPrice10, long? askVolume10,
        Money? bidPrice1, long? bidVolume1,
        Money? bidPrice2, long? bidVolume2,
        Money? bidPrice3, long? bidVolume3,
        Money? bidPrice4, long? bidVolume4,
        Money? bidPrice5, long? bidVolume5,
        Money? bidPrice6, long? bidVolume6,
        Money? bidPrice7, long? bidVolume7,
        Money? bidPrice8, long? bidVolume8,
        Money? bidPrice9, long? bidVolume9,
        Money? bidPrice10, long? bidVolume10)
    {
        // Ask levels
        AskPrice1 = askPrice1;
        AskVolume1 = askVolume1;
        AskPrice2 = askPrice2;
        AskVolume2 = askVolume2;
        AskPrice3 = askPrice3;
        AskVolume3 = askVolume3;
        AskPrice4 = askPrice4;
        AskVolume4 = askVolume4;
        AskPrice5 = askPrice5;
        AskVolume5 = askVolume5;
        AskPrice6 = askPrice6;
        AskVolume6 = askVolume6;
        AskPrice7 = askPrice7;
        AskVolume7 = askVolume7;
        AskPrice8 = askPrice8;
        AskVolume8 = askVolume8;
        AskPrice9 = askPrice9;
        AskVolume9 = askVolume9;
        AskPrice10 = askPrice10;
        AskVolume10 = askVolume10;

        // Bid levels
        BidPrice1 = bidPrice1;
        BidVolume1 = bidVolume1;
        BidPrice2 = bidPrice2;
        BidVolume2 = bidVolume2;
        BidPrice3 = bidPrice3;
        BidVolume3 = bidVolume3;
        BidPrice4 = bidPrice4;
        BidVolume4 = bidVolume4;
        BidPrice5 = bidPrice5;
        BidVolume5 = bidVolume5;
        BidPrice6 = bidPrice6;
        BidVolume6 = bidVolume6;
        BidPrice7 = bidPrice7;
        BidVolume7 = bidVolume7;
        BidPrice8 = bidPrice8;
        BidVolume8 = bidVolume8;
        BidPrice9 = bidPrice9;
        BidVolume9 = bidVolume9;
        BidPrice10 = bidPrice10;
        BidVolume10 = bidVolume10;

        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Calculate total volumes (총 잔량 계산)
    /// Details(상세설명) : Sums up all bid and ask volumes across 10 levels (10단계 매수/매도 잔량 합계 계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void CalculateTotals()
    {
        TotalBidVolume = (BidVolume1 ?? 0)
            + (BidVolume2 ?? 0)
            + (BidVolume3 ?? 0)
            + (BidVolume4 ?? 0)
            + (BidVolume5 ?? 0)
            + (BidVolume6 ?? 0)
            + (BidVolume7 ?? 0)
            + (BidVolume8 ?? 0)
            + (BidVolume9 ?? 0)
            + (BidVolume10 ?? 0);

        TotalAskVolume = (AskVolume1 ?? 0)
            + (AskVolume2 ?? 0)
            + (AskVolume3 ?? 0)
            + (AskVolume4 ?? 0)
            + (AskVolume5 ?? 0)
            + (AskVolume6 ?? 0)
            + (AskVolume7 ?? 0)
            + (AskVolume8 ?? 0)
            + (AskVolume9 ?? 0)
            + (AskVolume10 ?? 0);

        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Calculate bid-ask spread (매수-매도 스프레드 계산)
    /// Details(상세설명) : Returns the difference between best ask and best bid (최우선 매도호가와 매수호가의 차이 반환)
    /// </summary>
    public Money? GetSpread()
    {
        if (AskPrice1 == null || BidPrice1 == null)
            return null;

        return new Money(AskPrice1.Amount - BidPrice1.Amount, AskPrice1.Currency);
    }

    /// <summary>
    /// description(설명) : Calculate mid-price (중간 가격 계산)
    /// Details(상세설명) : Returns the average of best bid and best ask prices (최우선 매수/매도 호가의 평균 반환)
    /// </summary>
    public Money? GetMidPrice()
    {
        if (AskPrice1 == null || BidPrice1 == null)
            return null;

        var midAmount = (BidPrice1.Amount + AskPrice1.Amount) / 2;
        return new Money(midAmount, BidPrice1.Currency);
    }

    /// <summary>
    /// description(설명) : Check order book imbalance (호가 불균형 확인)
    /// Details(상세설명) : Returns positive value if more buy pressure, negative if more sell pressure (매수 압력이 크면 양수, 매도 압력이 크면 음수 반환)
    /// </summary>
    public decimal GetImbalanceRatio()
    {
        if (!TotalBidVolume.HasValue || !TotalAskVolume.HasValue)
            return 0;

        if (TotalBidVolume.Value + TotalAskVolume.Value == 0)
            return 0;

        return (decimal)(TotalBidVolume.Value - TotalAskVolume.Value) /
               (TotalBidVolume.Value + TotalAskVolume.Value);
    }
}

/// <summary>
/// description(설명) : Order Book ID value object (호가창 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for OrderBook (OrderBook의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class OrderBookId : GuidId
{
    public OrderBookId(Guid value) : base(value) { }

    public static OrderBookId New() => new(Guid.NewGuid());
}
