namespace AlgoTrading.Core.Exceptions.Risk;

/// <summary>
/// description : 포지션 크기가 허용된 한도를 초과했을 때 발생하는 예외
/// Details : Risk Management Context에서 단일 종목 또는 포트폴리오 전체의 포지션 크기가 RiskProfile의 MaxPositionSize를 초과할 때 발생합니다.
///           포지션 크기 한도는 절대 금액, 포트폴리오 대비 비율(%), 종목별 최대 수량 등 다양한 방식으로 설정될 수 있습니다.
///           이 예외는 특정 종목에 과도하게 집중 투자하는 것을 방지하여 포트폴리오 다각화를 강제하고 리스크를 분산시킵니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Risk Management Pattern, Position Sizing Pattern
/// </summary>
public class PositionSizeExceededException : DomainException
{
    public decimal MaxPositionSize { get; }
    public decimal RequestedSize { get; }
    public string? StockCode { get; }

    public PositionSizeExceededException(decimal maxPositionSize, decimal requestedSize)
        : base($"포지션 크기가 한도를 초과했습니다. MaxSize: {maxPositionSize:N2}, RequestedSize: {requestedSize:N2}",
            errorCode: "POSITION_SIZE_EXCEEDED")
    {
        MaxPositionSize = maxPositionSize;
        RequestedSize = requestedSize;
    }

    public PositionSizeExceededException(string stockCode, decimal maxPositionSize, decimal requestedSize)
        : base($"포지션 크기가 한도를 초과했습니다. StockCode: {stockCode}, MaxSize: {maxPositionSize:N2}, RequestedSize: {requestedSize:N2}",
            errorCode: "POSITION_SIZE_EXCEEDED")
    {
        StockCode = stockCode;
        MaxPositionSize = maxPositionSize;
        RequestedSize = requestedSize;
    }
}
