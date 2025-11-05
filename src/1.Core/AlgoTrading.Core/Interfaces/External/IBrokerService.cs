using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.External;

/// <summary>
/// description : 외부 증권사 브로커 API와 통신하는 어댑터 인터페이스
/// Details : 주문 전송, 취소, 수정 및 계좌/포지션 조회 등 브로커 API 기능을 추상화합니다.
///           키움 OpenAPI, eBest API 등 다양한 브로커를 지원하기 위한 Adapter Pattern을 적용했습니다.
/// Applied technology patterns : Adapter Pattern, Anti-Corruption Layer, Hexagonal Architecture, Dependency Inversion
/// </summary>
public interface IBrokerService
{
    /// <summary>
    /// 주문 전송
    /// </summary>
    Task<string> SubmitOrderAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 취소
    /// </summary>
    Task CancelOrderAsync(string brokerOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 수정
    /// </summary>
    Task ModifyOrderAsync(string brokerOrderId, int? newQuantity, decimal? newPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 상태 조회
    /// </summary>
    Task<Enums.OrderStatus> GetOrderStatusAsync(string brokerOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 계좌 잔고 조회
    /// </summary>
    Task<Account> GetAccountBalanceAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포지션 목록 조회
    /// </summary>
    Task<IEnumerable<Position>> GetPositionsAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 브로커 연결
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 브로커 연결 해제
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 연결 상태 확인
    /// </summary>
    Task<bool> IsConnectedAsync();
}
