namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : 여러 리포지토리 작업을 하나의 트랜잭션으로 묶어 관리하는 Unit of Work 패턴 인터페이스
/// Details : 모든 리포지토리에 대한 참조를 제공하고, 변경사항을 원자적으로 저장하며,
///           트랜잭션의 시작/커밋/롤백을 관리합니다. ACID 속성을 보장하는 핵심 인터페이스입니다.
/// Applied technology patterns : Unit of Work Pattern, Transaction Pattern, Repository Pattern, Dependency Injection
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// 주문 리포지토리
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// 전략 리포지토리
    /// </summary>
    IStrategyRepository Strategies { get; }

    /// <summary>
    /// 포트폴리오 리포지토리
    /// </summary>
    IPortfolioRepository Portfolios { get; }

    /// <summary>
    /// 계좌 리포지토리
    /// </summary>
    IAccountRepository Accounts { get; }

    /// <summary>
    /// 포지션 리포지토리
    /// </summary>
    IPositionRepository Positions { get; }

    /// <summary>
    /// 거래 리포지토리
    /// </summary>
    ITradeRepository Trades { get; }

    /// <summary>
    /// 리스크 프로필 리포지토리
    /// </summary>
    IRiskProfileRepository RiskProfiles { get; }

    /// <summary>
    /// 리스크 모니터 리포지토리
    /// </summary>
    IRiskMonitorRepository RiskMonitors { get; }

    /// <summary>
    /// 종목 리포지토리
    /// </summary>
    IStockRepository Stocks { get; }

    /// <summary>
    /// 가격 데이터 리포지토리
    /// </summary>
    IPriceDataRepository PriceData { get; }

    /// <summary>
    /// 백테스트 리포지토리
    /// </summary>
    IBacktestRepository Backtests { get; }

    /// <summary>
    /// 변경사항 저장
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 트랜잭션 시작
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 트랜잭션 커밋
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 트랜잭션 롤백
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
