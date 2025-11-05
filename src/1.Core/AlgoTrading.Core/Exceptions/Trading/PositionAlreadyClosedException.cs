namespace AlgoTrading.Core.Exceptions.Trading;

/// <summary>
/// description : 이미 청산된 포지션에 대해 작업을 시도할 때 발생하는 예외
/// Details : Position 엔티티는 Open/Closed 두 가지 상태를 가지며, 일단 Closed 상태가 되면 재청산, 수량 변경 등의 작업을 수행할 수 없습니다.
///           이미 청산된 포지션에 대해 Close() 메서드를 호출하거나, 청산된 포지션을 수정하려는 시도가 이 예외를 발생시킵니다.
///           이 예외는 포지션 생명주기의 불변성을 보호하며, 중복 청산으로 인한 데이터 불일치를 방지합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), State Machine Pattern, Invariant Protection
/// </summary>
public class PositionAlreadyClosedException : DomainException
{
    public DateTime ClosedAt { get; }

    public PositionAlreadyClosedException(Guid positionId, DateTime closedAt)
        : base($"포지션이 이미 청산되었습니다. PositionId: {positionId}, ClosedAt: {closedAt:yyyy-MM-dd HH:mm:ss}",
            positionId, "POSITION_ALREADY_CLOSED")
    {
        ClosedAt = closedAt;
    }
}
