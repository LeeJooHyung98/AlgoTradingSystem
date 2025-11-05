using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : Account 집합근(증권 계좌)에 대한 데이터 접근 인터페이스
/// Details : Account Context의 Account 엔티티를 관리하며, 계좌번호, 브로커, 계좌 유형(Cash/Margin), 잔고, 상태(Active/Suspended) 등의 정보를 포함하는 Aggregate Root입니다. 계좌번호는 유니크 식별자로 사용되며, 사용자는 여러 계좌를 보유할 수 있습니다. Active 계좌만 주문 생성이 가능하며, Suspended 계좌는 리스크 위반이나 규제 이슈로 거래가 일시 중지된 상태입니다. Trading Context는 주문 생성 시 이 인터페이스를 통해 계좌 상태와 잔고를 검증합니다.
/// Applied technology patterns : Repository Pattern, Aggregate Root Pattern, Multi-Tenancy Pattern
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// 계좌번호로 계좌 조회
    /// </summary>
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 사용자 ID로 계좌 목록 조회
    /// </summary>
    Task<IEnumerable<Account>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 계좌 상태로 계좌 목록 조회
    /// </summary>
    Task<IEnumerable<Account>> GetByStatusAsync(Enums.AccountStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 브로커 이름으로 계좌 목록 조회
    /// </summary>
    Task<IEnumerable<Account>> GetByBrokerAsync(string brokerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 활성화된 계좌 목록 조회
    /// </summary>
    Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
}
