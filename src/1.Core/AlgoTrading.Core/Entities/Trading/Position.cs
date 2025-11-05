using AlgoTrading.Core.ValueObjects;
using AlgoTrading.Core.DomainEvents;

namespace AlgoTrading.Core.Entities.Trading;

/// <summary>
/// description(설명) : Position Aggregate Root representing an open trading position (열린 거래 포지션을 나타내는 포지션 집합 루트)
/// Details(상세설명) : Manages position quantity, entry/exit prices, P&L, and risk management with stop loss and take profit (포지션 수량, 진입/청산 가격, 손익, 손절 및 익절을 포함한 리스크 관리)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern, Domain-Driven Design, Domain Events (집합 루트 패턴, 도메인 주도 설계, 도메인 이벤트)
/// </summary>
public sealed class Position : AggregateRoot<PositionId>
{
    /// <summary>
    /// 종목 코드
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// 포지션 방향 (롱/숏)
    /// </summary>
    public PositionSide Side { get; private set; }

    /// <summary>
    /// 현재 수량
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// 평균 진입 가격
    /// </summary>
    public decimal AverageEntryPrice { get; private set; }

    /// <summary>
    /// 현재 시장 가격
    /// </summary>
    public decimal CurrentPrice { get; private set; }

    /// <summary>
    /// 진입 주문 ID
    /// </summary>
    public OrderId EntryOrderId { get; private set; }

    /// <summary>
    /// 손절 가격
    /// </summary>
    public decimal? StopLossPrice { get; private set; }

    /// <summary>
    /// 익절 가격
    /// </summary>
    public decimal? TakeProfitPrice { get; private set; }

    /// <summary>
    /// 포지션 오픈 시간
    /// </summary>
    public DateTime OpenedAt { get; private set; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; private set; }

    /// <summary>
    /// 전략 ID (전략에서 발생한 경우)
    /// </summary>
    public Guid? StrategyId { get; private set; }

    /// <summary>
    /// 총 비용 기준
    /// </summary>
    public Money CostBasis => new Money(AverageEntryPrice * Quantity);

    /// <summary>
    /// 현재 시장 가치
    /// </summary>
    public Money MarketValue => new Money(CurrentPrice * Quantity);

    /// <summary>
    /// 미실현 손익
    /// </summary>
    public Money UnrealizedPL => Side == PositionSide.Long
        ? MarketValue - CostBasis
        : CostBasis - MarketValue;

    /// <summary>
    /// 미실현 손익 비율
    /// </summary>
    public decimal UnrealizedPLPercent => CostBasis.Amount != 0
        ? (UnrealizedPL.Amount / CostBasis.Amount) * 100
        : 0;

    private Position() { } // EF Core

    private Position(
        PositionId id,
        StockCode stockCode,
        PositionSide side,
        int quantity,
        decimal averageEntryPrice,
        OrderId entryOrderId,
        string accountNumber,
        Guid? strategyId = null)
    {
        Id = id;
        StockCode = stockCode;
        Side = side;
        Quantity = quantity;
        AverageEntryPrice = averageEntryPrice;
        CurrentPrice = averageEntryPrice;
        EntryOrderId = entryOrderId;
        AccountNumber = accountNumber;
        StrategyId = strategyId;
        OpenedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Open a new position (새로운 포지션 오픈)
    /// Details(상세설명) : Factory method to create a new position with entry price and optional risk management settings (진입 가격과 선택적 리스크 관리 설정으로 새 포지션을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design (팩토리 패턴, 도메인 주도 설계)
    /// </summary>
    /// <returns>Created position instance (생성된 포지션 인스턴스)</returns>
    public static Position Open(
        StockCode stockCode,
        PositionSide side,
        int quantity,
        decimal entryPrice,
        OrderId entryOrderId,
        string accountNumber,
        Guid? strategyId = null,
        decimal? stopLossPrice = null,
        decimal? takeProfitPrice = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (entryPrice <= 0)
            throw new ArgumentException("Entry price must be positive", nameof(entryPrice));

        var id = new PositionId(Guid.NewGuid());
        var position = new Position(id, stockCode, side, quantity, entryPrice, entryOrderId, accountNumber, strategyId);

        if (stopLossPrice.HasValue)
            position.SetStopLoss(stopLossPrice.Value);

        if (takeProfitPrice.HasValue)
            position.SetTakeProfit(takeProfitPrice.Value);

        return position;
    }

    /// <summary>
    /// description(설명) : Update current market price (현재 시장 가격 업데이트)
    /// Details(상세설명) : Updates position price and checks if stop loss or take profit is triggered (포지션 가격을 업데이트하고 손절 또는 익절이 발동되었는지 확인)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Domain Events (도메인 주도 설계, 도메인 이벤트)
    /// </summary>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be positive", nameof(newPrice));

        var previousPrice = CurrentPrice;
        CurrentPrice = newPrice;

        // Check if stop loss or take profit triggered
        CheckStopLossTriggered();
        CheckTakeProfitTriggered();

        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Increase position size (averaging in) (포지션 크기 증가 - 물타기)
    /// Details(상세설명) : Adds quantity to position and recalculates average entry price (포지션에 수량을 추가하고 평균 진입 가격을 재계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void IncreasePosition(int additionalQuantity, decimal price)
    {
        if (additionalQuantity <= 0)
            throw new ArgumentException("Additional quantity must be positive", nameof(additionalQuantity));

        if (price <= 0)
            throw new ArgumentException("Price must be positive", nameof(price));

        // Calculate new average entry price
        var totalCost = (AverageEntryPrice * Quantity) + (price * additionalQuantity);
        var totalQuantity = Quantity + additionalQuantity;
        AverageEntryPrice = totalCost / totalQuantity;
        Quantity = totalQuantity;

        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Decrease position size (partial close) (포지션 크기 감소 - 부분 청산)
    /// Details(상세설명) : Closes partial quantity and calculates realized P&L, raises event if fully closed (일부 수량을 청산하고 실현 손익을 계산, 완전 청산 시 이벤트 발생)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Domain Events (도메인 주도 설계, 도메인 이벤트)
    /// </summary>
    /// <returns>Realized profit or loss from closed portion (청산된 부분의 실현 손익)</returns>
    public Money DecreasePosition(int quantityToClose, decimal exitPrice)
    {
        if (quantityToClose <= 0 || quantityToClose > Quantity)
            throw new ArgumentException($"Invalid quantity to close: {quantityToClose}. Current: {Quantity}", nameof(quantityToClose));

        if (exitPrice <= 0)
            throw new ArgumentException("Exit price must be positive", nameof(exitPrice));

        // Calculate realized P&L for the closed portion
        var realizedPL = Side == PositionSide.Long
            ? new Money((exitPrice - AverageEntryPrice) * quantityToClose)
            : new Money((AverageEntryPrice - exitPrice) * quantityToClose);

        Quantity -= quantityToClose;

        if (Quantity == 0)
        {
            // Position fully closed
            AddDomainEvent(new PositionClosedEvent(Id, StockCode, realizedPL, DateTime.UtcNow));
        }

        MarkAsModified();
        return realizedPL;
    }

    /// <summary>
    /// description(설명) : Set stop loss price (손절 가격 설정)
    /// Details(상세설명) : Configures stop loss price with validation for position side (포지션 방향에 대한 검증과 함께 손절 가격을 설정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Value Validation (도메인 주도 설계, 값 검증)
    /// </summary>
    public void SetStopLoss(decimal stopLossPrice)
    {
        ValidateStopLoss(stopLossPrice);
        StopLossPrice = stopLossPrice;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Set take profit price (익절 가격 설정)
    /// Details(상세설명) : Configures take profit price with validation for position side (포지션 방향에 대한 검증과 함께 익절 가격을 설정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Value Validation (도메인 주도 설계, 값 검증)
    /// </summary>
    public void SetTakeProfit(decimal takeProfitPrice)
    {
        ValidateTakeProfit(takeProfitPrice);
        TakeProfitPrice = takeProfitPrice;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update trailing stop loss (추적 손절 업데이트)
    /// Details(상세설명) : Adjusts stop loss to trail current price by specified amount (지정된 금액만큼 현재 가격을 추적하도록 손절을 조정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateTrailingStopLoss(decimal trailingAmount)
    {
        if (Side == PositionSide.Long)
        {
            var newStopLoss = CurrentPrice - trailingAmount;
            if (!StopLossPrice.HasValue || newStopLoss > StopLossPrice.Value)
            {
                SetStopLoss(newStopLoss);
            }
        }
        else // Short
        {
            var newStopLoss = CurrentPrice + trailingAmount;
            if (!StopLossPrice.HasValue || newStopLoss < StopLossPrice.Value)
            {
                SetStopLoss(newStopLoss);
            }
        }
    }

    /// <summary>
    /// description(설명) : Check if position is profitable (포지션이 수익 중인지 확인)
    /// Details(상세설명) : Returns true if unrealized P&L is positive (미실현 손익이 양수이면 true 반환)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>True if position has profit (포지션이 수익 중이면 true)</returns>
    public bool IsProfitable()
    {
        return UnrealizedPL.Amount > 0;
    }

    /// <summary>
    /// description(설명) : Check if position is in loss (포지션이 손실 중인지 확인)
    /// Details(상세설명) : Returns true if unrealized P&L is negative (미실현 손익이 음수이면 true 반환)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>True if position has loss (포지션이 손실 중이면 true)</returns>
    public bool IsInLoss()
    {
        return UnrealizedPL.Amount < 0;
    }

    /// <summary>
    /// description(설명) : Get position holding duration (포지션 보유 기간 조회)
    /// Details(상세설명) : Calculates time elapsed since position was opened (포지션 오픈 이후 경과 시간을 계산)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>Time span since position opened (포지션 오픈 이후 시간)</returns>
    public TimeSpan GetDuration()
    {
        return DateTime.UtcNow - OpenedAt;
    }

    /// <summary>
    /// description(설명) : Get risk amount based on stop loss (손절 기반 리스크 금액 조회)
    /// Details(상세설명) : Calculates potential loss if stop loss is triggered (손절 발동 시 잠재적 손실을 계산)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern, Value Object Pattern (쿼리 메서드 패턴, 값 객체 패턴)
    /// </summary>
    /// <returns>Risk amount or null if no stop loss set (리스크 금액, 손절 미설정 시 null)</returns>
    public Money? GetRiskAmount()
    {
        if (!StopLossPrice.HasValue)
            return null;

        var riskPerShare = Math.Abs(AverageEntryPrice - StopLossPrice.Value);
        return new Money(riskPerShare * Quantity);
    }

    /// <summary>
    /// description(설명) : Get risk-reward ratio (리스크-보상 비율 조회)
    /// Details(상세설명) : Calculates ratio of potential reward to risk based on stop loss and take profit (손절과 익절 기반 잠재적 보상 대 리스크 비율을 계산)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>Risk-reward ratio or null if not set (리스크-보상 비율, 미설정 시 null)</returns>
    public decimal? GetRiskRewardRatio()
    {
        if (!StopLossPrice.HasValue || !TakeProfitPrice.HasValue)
            return null;

        var risk = Math.Abs(AverageEntryPrice - StopLossPrice.Value);
        var reward = Math.Abs(TakeProfitPrice.Value - AverageEntryPrice);

        return risk > 0 ? reward / risk : null;
    }

    private void ValidateStopLoss(decimal stopLossPrice)
    {
        if (stopLossPrice <= 0)
            throw new ArgumentException("Stop loss price must be positive", nameof(stopLossPrice));

        if (Side == PositionSide.Long && stopLossPrice >= AverageEntryPrice)
            throw new ArgumentException("For long positions, stop loss must be below entry price");

        if (Side == PositionSide.Short && stopLossPrice <= AverageEntryPrice)
            throw new ArgumentException("For short positions, stop loss must be above entry price");
    }

    private void ValidateTakeProfit(decimal takeProfitPrice)
    {
        if (takeProfitPrice <= 0)
            throw new ArgumentException("Take profit price must be positive", nameof(takeProfitPrice));

        if (Side == PositionSide.Long && takeProfitPrice <= AverageEntryPrice)
            throw new ArgumentException("For long positions, take profit must be above entry price");

        if (Side == PositionSide.Short && takeProfitPrice >= AverageEntryPrice)
            throw new ArgumentException("For short positions, take profit must be below entry price");
    }

    private void CheckStopLossTriggered()
    {
        if (!StopLossPrice.HasValue)
            return;

        bool triggered = Side == PositionSide.Long
            ? CurrentPrice <= StopLossPrice.Value
            : CurrentPrice >= StopLossPrice.Value;

        if (triggered)
        {
            AddDomainEvent(new StopLossTriggeredEvent(Id, StockCode, CurrentPrice, StopLossPrice.Value, DateTime.UtcNow));
        }
    }

    private void CheckTakeProfitTriggered()
    {
        if (!TakeProfitPrice.HasValue)
            return;

        bool triggered = Side == PositionSide.Long
            ? CurrentPrice >= TakeProfitPrice.Value
            : CurrentPrice <= TakeProfitPrice.Value;

        if (triggered)
        {
            AddDomainEvent(new TakeProfitTriggeredEvent(Id, StockCode, CurrentPrice, TakeProfitPrice.Value, DateTime.UtcNow));
        }
    }
}

/// <summary>
/// description(설명) : Position ID value object (포지션 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for Position aggregate (Position 집합의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class PositionId : GuidId
{
    public PositionId(Guid value) : base(value) { }

    public static PositionId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Position Side enumeration (포지션 방향 열거형)
/// Details(상세설명) : Defines whether position is long or short (포지션이 롱인지 숏인지 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum PositionSide
{
    Long = 1,   // Long position (매수 포지션)
    Short = 2   // Short position (공매도 포지션)
}

// Domain Events
public sealed class PositionClosedEvent : DomainEvent
{
    public PositionId PositionId { get; }
    public StockCode StockCode { get; }
    public Money RealizedPL { get; }

    public PositionClosedEvent(PositionId positionId, StockCode stockCode, Money realizedPL, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        PositionId = positionId;
        StockCode = stockCode;
        RealizedPL = realizedPL;
    }
}

public sealed class StopLossTriggeredEvent : DomainEvent
{
    public PositionId PositionId { get; }
    public StockCode StockCode { get; }
    public decimal CurrentPrice { get; }
    public decimal StopLossPrice { get; }

    public StopLossTriggeredEvent(PositionId positionId, StockCode stockCode, decimal currentPrice, decimal stopLossPrice, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        PositionId = positionId;
        StockCode = stockCode;
        CurrentPrice = currentPrice;
        StopLossPrice = stopLossPrice;
    }
}

public sealed class TakeProfitTriggeredEvent : DomainEvent
{
    public PositionId PositionId { get; }
    public StockCode StockCode { get; }
    public decimal CurrentPrice { get; }
    public decimal TakeProfitPrice { get; }

    public TakeProfitTriggeredEvent(PositionId positionId, StockCode stockCode, decimal currentPrice, decimal takeProfitPrice, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        PositionId = positionId;
        StockCode = stockCode;
        CurrentPrice = currentPrice;
        TakeProfitPrice = takeProfitPrice;
    }
}
