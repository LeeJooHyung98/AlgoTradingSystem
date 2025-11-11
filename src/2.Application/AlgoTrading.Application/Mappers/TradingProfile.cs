using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Core.Entities.Trading;
using Mapster;

namespace AlgoTrading.Application.Mappers;

/// <summary>
/// description(설명) : Trading Mapster Profile (Trading Mapster 프로필)
/// Details(상세설명) : Configures Mapster mappings for Trading entities to DTOs (Trading 엔티티를 DTO로 매핑하는 Mapster 설정)
/// Applied technology patterns(적용기술패턴) : Mapper Pattern, Configuration Pattern (매퍼 패턴, 설정 패턴)
/// </summary>
public sealed class TradingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Order -> OrderDto mapping
        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.StockCode, src => src.StockCode.Value)
            .Map(dest => dest.Side, src => (int)src.Side)
            .Map(dest => dest.SideName, src => src.Side.ToString())
            .Map(dest => dest.Type, src => (int)src.Type)
            .Map(dest => dest.TypeName, src => src.Type.ToString())
            .Map(dest => dest.Status, src => (int)src.Status)
            .Map(dest => dest.StatusName, src => src.Status.ToString())
            .Map(dest => dest.TimeInForce, src => (int)src.TimeInForce)
            .Map(dest => dest.TimeInForceName, src => src.TimeInForce.ToString())
            .Map(dest => dest.Source, src => (int)src.Source)
            .Map(dest => dest.SourceName, src => src.Source.ToString())
            .Map(dest => dest.FillRate, src => src.GetFillRate())
            .Map(dest => dest.EstimatedValue, src => src.GetEstimatedValue().Amount)
            .Map(dest => dest.IsActive, src => src.IsActive())
            .Map(dest => dest.IsCompleted, src => src.IsCompleted());

        // Position -> PositionDto mapping
        config.NewConfig<Position, PositionDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.StockCode, src => src.StockCode.Value)
            .Map(dest => dest.Side, src => (int)src.Side)
            .Map(dest => dest.SideName, src => src.Side.ToString())
            .Map(dest => dest.EntryOrderId, src => src.EntryOrderId.Value)
            .Map(dest => dest.CostBasis, src => src.CostBasis.Amount)
            .Map(dest => dest.MarketValue, src => src.MarketValue.Amount)
            .Map(dest => dest.UnrealizedPL, src => src.UnrealizedPL.Amount)
            .Map(dest => dest.UnrealizedPLPercent, src => src.UnrealizedPLPercent)
            .Map(dest => dest.IsProfitable, src => src.IsProfitable())
            .Map(dest => dest.IsInLoss, src => src.IsInLoss())
            .Map(dest => dest.DurationInHours, src => src.GetDuration().TotalHours)
            .Map(dest => dest.DurationInDays, src => src.GetDuration().TotalDays)
            .Map(dest => dest.RiskAmount, src => src.GetRiskAmount() != null ? src.GetRiskAmount()!.Amount : (decimal?)null)
            .Map(dest => dest.RiskRewardRatio, src => src.GetRiskRewardRatio());

        // Trade -> TradeDto mapping
        config.NewConfig<Trade, TradeDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.StockCode, src => src.StockCode.Value)
            .Map(dest => dest.Side, src => (int)src.Side)
            .Map(dest => dest.SideName, src => src.Side.ToString())
            .Map(dest => dest.EntryOrderId, src => src.EntryOrderId.Value)
            .Map(dest => dest.ExitOrderId, src => src.ExitOrderId.Value)
            .Map(dest => dest.RealizedPL, src => src.RealizedPL.Amount)
            .Map(dest => dest.Commission, src => src.Commission.Amount)
            .Map(dest => dest.DurationInHours, src => src.GetDurationInHours())
            .Map(dest => dest.DurationInDays, src => src.GetDurationInDays())
            .Map(dest => dest.Result, src => (int)src.Result)
            .Map(dest => dest.ResultName, src => src.Result.ToString())
            .Map(dest => dest.IsWinner, src => src.IsWinner())
            .Map(dest => dest.IsLoser, src => src.IsLoser())
            .Map(dest => dest.IsDayTrade, src => src.IsDayTrade())
            .Map(dest => dest.GrossPL, src => src.GetGrossPL().Amount);

        // Execution -> ExecutionDto mapping
        config.NewConfig<Execution, ExecutionDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.OrderId, src => src.OrderId.Value)
            .Map(dest => dest.StockCode, src => src.StockCode.Value)
            .Map(dest => dest.Side, src => (int)src.Side)
            .Map(dest => dest.SideName, src => src.Side.ToString())
            .Map(dest => dest.Value, src => src.Value.Amount)
            .Map(dest => dest.Commission, src => src.Commission.Amount)
            .Map(dest => dest.NetValue, src => src.GetNetValue().Amount)
            .Map(dest => dest.EffectivePrice, src => src.GetEffectivePrice());
    }
}
