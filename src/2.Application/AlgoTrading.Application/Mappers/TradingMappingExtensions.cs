using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Core.Entities.Trading;

namespace AlgoTrading.Application.Mappers;

/// <summary>
/// description(설명) : Trading Entity to DTO Mapping Extensions (Trading 엔티티를 DTO로 매핑하는 확장 메서드)
/// Details(상세설명) : Extension methods for manual entity to DTO mapping (수동 엔티티-DTO 매핑을 위한 확장 메서드)
/// Applied technology patterns(적용기술패턴) : Extension Methods Pattern, Mapper Pattern (확장 메서드 패턴, 매퍼 패턴)
/// </summary>
/// <remarks>
/// Note: This is a temporary manual mapping solution. Will be replaced by Mapster/AutoMapper profiles.
/// </remarks>
public static class TradingMappingExtensions
{
    /// <summary>
    /// Order 엔티티를 OrderDto로 매핑
    /// </summary>
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id.Value,
            StockCode = order.StockCode.Value,
            Side = (int)order.Side,
            SideName = order.Side.ToString(),
            Type = (int)order.Type,
            TypeName = order.Type.ToString(),
            Quantity = order.Quantity,
            FilledQuantity = order.FilledQuantity,
            RemainingQuantity = order.RemainingQuantity,
            LimitPrice = order.LimitPrice,
            StopPrice = order.StopPrice,
            AverageFillPrice = order.AverageFillPrice,
            Status = (int)order.Status,
            StatusName = order.Status.ToString(),
            TimeInForce = (int)order.TimeInForce,
            TimeInForceName = order.TimeInForce.ToString(),
            SubmittedAt = order.SubmittedAt,
            AcceptedAt = order.AcceptedAt,
            CompletedAt = order.CompletedAt,
            CancelledAt = order.CancelledAt,
            BrokerOrderId = order.BrokerOrderId,
            AccountNumber = order.AccountNumber,
            Source = (int)order.Source,
            SourceName = order.Source.ToString(),
            StrategyId = order.StrategyId,
            CancellationReason = order.CancellationReason,
            RejectionReason = order.RejectionReason,
            FillRate = order.GetFillRate(),
            EstimatedValue = order.GetEstimatedValue().Amount,
            IsActive = order.IsActive(),
            IsCompleted = order.IsCompleted()
        };
    }

    /// <summary>
    /// Position 엔티티를 PositionDto로 매핑
    /// </summary>
    public static PositionDto ToDto(this Position position)
    {
        return new PositionDto
        {
            Id = position.Id.Value,
            StockCode = position.StockCode.Value,
            Side = (int)position.Side,
            SideName = position.Side.ToString(),
            Quantity = position.Quantity,
            AverageEntryPrice = position.AverageEntryPrice,
            CurrentPrice = position.CurrentPrice,
            EntryOrderId = position.EntryOrderId.Value,
            StopLossPrice = position.StopLossPrice,
            TakeProfitPrice = position.TakeProfitPrice,
            OpenedAt = position.OpenedAt,
            AccountNumber = position.AccountNumber,
            StrategyId = position.StrategyId,
            CostBasis = position.CostBasis.Amount,
            MarketValue = position.MarketValue.Amount,
            UnrealizedPL = position.UnrealizedPL.Amount,
            UnrealizedPLPercent = position.UnrealizedPLPercent,
            IsProfitable = position.IsProfitable(),
            IsInLoss = position.IsInLoss(),
            DurationInHours = position.GetDuration().TotalHours,
            DurationInDays = position.GetDuration().TotalDays,
            RiskAmount = position.GetRiskAmount()?.Amount,
            RiskRewardRatio = position.GetRiskRewardRatio()
        };
    }

    /// <summary>
    /// Trade 엔티티를 TradeDto로 매핑
    /// </summary>
    public static TradeDto ToDto(this Trade trade)
    {
        return new TradeDto
        {
            Id = trade.Id.Value,
            StockCode = trade.StockCode.Value,
            Side = (int)trade.Side,
            SideName = trade.Side.ToString(),
            EntryOrderId = trade.EntryOrderId.Value,
            ExitOrderId = trade.ExitOrderId.Value,
            Quantity = trade.Quantity,
            EntryPrice = trade.EntryPrice,
            ExitPrice = trade.ExitPrice,
            EntryTime = trade.EntryTime,
            ExitTime = trade.ExitTime,
            RealizedPL = trade.RealizedPL.Amount,
            RealizedPLPercent = trade.RealizedPLPercent,
            Commission = trade.Commission.Amount,
            DurationInHours = trade.GetDurationInHours(),
            DurationInDays = trade.GetDurationInDays(),
            AccountNumber = trade.AccountNumber,
            StrategyId = trade.StrategyId,
            Result = (int)trade.Result,
            ResultName = trade.Result.ToString(),
            Notes = trade.Notes,
            IsWinner = trade.IsWinner(),
            IsLoser = trade.IsLoser(),
            IsDayTrade = trade.IsDayTrade(),
            GrossPL = trade.GetGrossPL().Amount
        };
    }

    /// <summary>
    /// Execution 엔티티를 ExecutionDto로 매핑
    /// </summary>
    public static ExecutionDto ToDto(this Execution execution)
    {
        return new ExecutionDto
        {
            Id = execution.Id.Value,
            OrderId = execution.OrderId.Value,
            StockCode = execution.StockCode.Value,
            Side = (int)execution.Side,
            SideName = execution.Side.ToString(),
            Quantity = execution.Quantity,
            Price = execution.Price,
            Value = execution.Value.Amount,
            Commission = execution.Commission.Amount,
            ExecutionTime = execution.ExecutionTime,
            BrokerExecutionId = execution.BrokerExecutionId,
            ExecutionVenue = execution.ExecutionVenue,
            IsPartialFill = execution.IsPartialFill,
            AccountNumber = execution.AccountNumber,
            NetValue = execution.GetNetValue().Amount,
            EffectivePrice = execution.GetEffectivePrice()
        };
    }
}
