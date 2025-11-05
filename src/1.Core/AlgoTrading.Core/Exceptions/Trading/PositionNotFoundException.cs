namespace AlgoTrading.Core.Exceptions.Trading;

/// <summary>
/// description : 요청한 포지션 ID를 찾을 수 없을 때 발생하는 예외
/// Details : Trading Context에서 존재하지 않는 포지션을 조회, 수정, 청산하려고 할 때 발생합니다.
///           Position은 Order와 달리 장기간 유지될 수 있으므로, 잘못된 PositionId 참조는 전략 실행 오류로 이어질 수 있습니다.
///           이 예외는 포지션 관리 시스템의 데이터 일관성을 보장하며, 클라이언트에게 404 Not Found 응답을 반환하는 데 사용됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Pattern, Not Found Pattern
/// </summary>
public class PositionNotFoundException : DomainException
{
    public PositionNotFoundException(Guid positionId)
        : base($"포지션을 찾을 수 없습니다. PositionId: {positionId}", positionId, "POSITION_NOT_FOUND")
    {
    }

    public PositionNotFoundException(Guid accountId, string stockCode)
        : base($"포지션을 찾을 수 없습니다. AccountId: {accountId}, StockCode: {stockCode}", accountId, "POSITION_NOT_FOUND")
    {
    }
}
