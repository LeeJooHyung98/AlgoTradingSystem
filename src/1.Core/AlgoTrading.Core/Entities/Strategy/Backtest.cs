using AlgoTrading.Core.ValueObjects;
using AlgoTrading.Core.DomainEvents;

namespace AlgoTrading.Core.Entities.Strategy;

/// <summary>
/// description(설명) : Backtest Aggregate Root - Represents a backtest run of a trading strategy (백테스트 집합 루트 - 트레이딩 전략의 백테스트 실행을 나타냄)
/// Details(상세설명) : Manages strategy backtest execution with historical data, performance metrics, and result analysis (과거 데이터로 전략 백테스트 실행 및 성과 지표, 결과 분석 관리)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern (집합 루트 패턴), Domain-Driven Design (도메인 주도 설계), Domain Events (도메인 이벤트), State Machine Pattern (상태 머신 패턴)
/// </summary>
public sealed class Backtest : AggregateRoot<BacktestId>
{
    /// <summary>
    /// 테스트 대상 전략 ID
    /// </summary>
    public StrategyId StrategyId { get; private set; }

    /// <summary>
    /// 백테스트 이름
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 백테스트 시작 날짜
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// 백테스트 종료 날짜
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// 초기 자본금
    /// </summary>
    public Money InitialCapital { get; private set; }

    /// <summary>
    /// 최종 자본금
    /// </summary>
    public Money? FinalCapital { get; private set; }

    /// <summary>
    /// 총 수익금
    /// </summary>
    public decimal? TotalReturn { get; private set; }

    /// <summary>
    /// 총 수익률 (%)
    /// </summary>
    public decimal? TotalReturnPercent { get; private set; }

    /// <summary>
    /// 총 거래 횟수
    /// </summary>
    public int TotalTrades { get; private set; }

    /// <summary>
    /// 수익 거래 횟수
    /// </summary>
    public int WinningTrades { get; private set; }

    /// <summary>
    /// 손실 거래 횟수
    /// </summary>
    public int LosingTrades { get; private set; }

    /// <summary>
    /// 승률 (%)
    /// </summary>
    public decimal WinRate => TotalTrades > 0 ? (decimal)WinningTrades / TotalTrades * 100 : 0;

    /// <summary>
    /// 평균 수익
    /// </summary>
    public Money? AverageWin { get; private set; }

    /// <summary>
    /// 평균 손실
    /// </summary>
    public Money? AverageLoss { get; private set; }

    /// <summary>
    /// 최대 수익
    /// </summary>
    public Money? LargestWin { get; private set; }

    /// <summary>
    /// 최대 손실
    /// </summary>
    public Money? LargestLoss { get; private set; }

    /// <summary>
    /// 손익 비율 (수익/손실)
    /// </summary>
    public decimal? ProfitFactor { get; private set; }

    /// <summary>
    /// 샤프 비율 (위험 대비 수익률)
    /// </summary>
    public decimal? SharpeRatio { get; private set; }

    /// <summary>
    /// 최대 낙폭 (MDD)
    /// </summary>
    public decimal? MaxDrawdown { get; private set; }

    /// <summary>
    /// 최대 낙폭 비율 (%)
    /// </summary>
    public decimal? MaxDrawdownPercent { get; private set; }

    /// <summary>
    /// 백테스트 상태
    /// </summary>
    public BacktestStatus Status { get; private set; }

    /// <summary>
    /// 백테스트 시작 시간
    /// </summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>
    /// 백테스트 완료 시간
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 오류 메시지 (실패한 경우)
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// 진행률 (0-100)
    /// </summary>
    public int ProgressPercent { get; private set; }

    private Backtest() { }

    private Backtest(
        BacktestId id,
        StrategyId strategyId,
        string name,
        DateTime startDate,
        DateTime endDate,
        Money initialCapital)
    {
        Id = id;
        StrategyId = strategyId;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        InitialCapital = initialCapital;
        Status = BacktestStatus.Pending;
        ProgressPercent = 0;
    }

    /// <summary>
    /// description(설명) : Create a new backtest (새 백테스트 생성)
    /// Details(상세설명) : Factory method to create a backtest for strategy performance evaluation (전략 성과 평가를 위한 백테스트 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계), Domain Events (도메인 이벤트)
    /// </summary>
    /// <returns>Newly created Backtest instance (새로 생성된 백테스트 인스턴스)</returns>
    public static Backtest Create(
        StrategyId strategyId,
        string name,
        DateTime startDate,
        DateTime endDate,
        Money initialCapital)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date");

        if (initialCapital.Amount <= 0)
            throw new ArgumentException("Initial capital must be positive");

        var id = new BacktestId(Guid.NewGuid());
        var backtest = new Backtest(id, strategyId, name, startDate, endDate, initialCapital);

        backtest.AddDomainEvent(new BacktestCreatedEvent(id, strategyId, startDate, endDate, DateTime.UtcNow));

        return backtest;
    }

    /// <summary>
    /// description(설명) : Start the backtest execution (백테스트 실행 시작)
    /// Details(상세설명) : Changes status to running and raises domain event for backtest initiation (상태를 실행 중으로 변경하고 백테스트 시작 도메인 이벤트 발생)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴), Domain Events (도메인 이벤트), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Start()
    {
        if (Status != BacktestStatus.Pending)
            throw new InvalidOperationException($"Cannot start backtest in {Status} status");

        Status = BacktestStatus.Running;
        StartedAt = DateTime.UtcNow;
        ProgressPercent = 0;

        AddDomainEvent(new BacktestStartedEvent(Id, StrategyId, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// Update backtest progress
    /// </summary>
    public void UpdateProgress(int percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentException("Progress must be between 0 and 100", nameof(percent));

        ProgressPercent = percent;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Complete the backtest with results (백테스트를 결과와 함께 완료)
    /// Details(상세설명) : Finalizes backtest with performance metrics and raises completion event (성과 지표와 함께 백테스트를 종료하고 완료 이벤트 발생)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴), Domain Events (도메인 이벤트), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Complete(BacktestResults results)
    {
        if (Status != BacktestStatus.Running)
            throw new InvalidOperationException($"Cannot complete backtest in {Status} status");

        Status = BacktestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ProgressPercent = 100;

        // Set results
        FinalCapital = results.FinalCapital;
        TotalReturn = (results.FinalCapital - InitialCapital).Amount;
        TotalReturnPercent = InitialCapital.Amount > 0
            ? ((results.FinalCapital.Amount - InitialCapital.Amount) / InitialCapital.Amount) * 100
            : 0;
        TotalTrades = results.TotalTrades;
        WinningTrades = results.WinningTrades;
        LosingTrades = results.LosingTrades;
        AverageWin = results.AverageWin;
        AverageLoss = results.AverageLoss;
        LargestWin = results.LargestWin;
        LargestLoss = results.LargestLoss;
        ProfitFactor = results.ProfitFactor;
        SharpeRatio = results.SharpeRatio;
        MaxDrawdown = results.MaxDrawdown?.Amount;
        MaxDrawdownPercent = results.MaxDrawdownPercent;

        AddDomainEvent(new BacktestCompletedEvent(Id, StrategyId, TotalReturnPercent.Value, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// Mark backtest as failed
    /// </summary>
    public void Fail(string errorMessage)
    {
        Status = BacktestStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;

        AddDomainEvent(new BacktestFailedEvent(Id, StrategyId, errorMessage, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// Cancel the backtest
    /// </summary>
    public void Cancel()
    {
        if (Status != BacktestStatus.Running)
            throw new InvalidOperationException($"Cannot cancel backtest in {Status} status");

        Status = BacktestStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// Get backtest duration
    /// </summary>
    public TimeSpan? GetDuration()
    {
        if (!StartedAt.HasValue)
            return null;

        var endTime = CompletedAt ?? DateTime.UtcNow;
        return endTime - StartedAt.Value;
    }
}

/// <summary>
/// description(설명) : Backtest identifier value object (백테스트 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for Backtest entity using GUID (GUID를 사용한 백테스트 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class BacktestId : GuidId
{
    public BacktestId(Guid value) : base(value) { }

    public static BacktestId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Backtest status enumeration (백테스트 상태 열거형)
/// Details(상세설명) : Tracks the lifecycle state of a backtest execution (백테스트 실행의 생명주기 상태 추적)
/// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum BacktestStatus
{
    Pending = 1,                // Waiting to start (시작 대기 중)
    Running = 2,                // Currently running (현재 실행 중)
    Completed = 3,              // Successfully completed (성공적으로 완료됨)
    Failed = 4,                 // Failed with error (오류로 실패함)
    Cancelled = 5               // Cancelled by user (사용자가 취소함)
}

/// <summary>
/// description(설명) : Backtest Results Value Object (백테스트 결과 값 객체)
/// Details(상세설명) : Contains all performance metrics and statistics from backtest execution (백테스트 실행의 모든 성과 지표와 통계 포함)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class BacktestResults
{
    public Money FinalCapital { get; set; }
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public Money? AverageWin { get; set; }
    public Money? AverageLoss { get; set; }
    public Money? LargestWin { get; set; }
    public Money? LargestLoss { get; set; }
    public decimal? ProfitFactor { get; set; }
    public decimal? SharpeRatio { get; set; }
    public Money? MaxDrawdown { get; set; }
    public decimal? MaxDrawdownPercent { get; set; }
}

// Domain Events
public sealed class BacktestCreatedEvent : DomainEvent
{
    public BacktestId BacktestId { get; }
    public StrategyId StrategyId { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    public BacktestCreatedEvent(BacktestId backtestId, StrategyId strategyId, DateTime startDate, DateTime endDate, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        BacktestId = backtestId;
        StrategyId = strategyId;
        StartDate = startDate;
        EndDate = endDate;
    }
}

public sealed class BacktestStartedEvent : DomainEvent
{
    public BacktestId BacktestId { get; }
    public StrategyId StrategyId { get; }

    public BacktestStartedEvent(BacktestId backtestId, StrategyId strategyId, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        BacktestId = backtestId;
        StrategyId = strategyId;
    }
}

public sealed class BacktestCompletedEvent : DomainEvent
{
    public BacktestId BacktestId { get; }
    public StrategyId StrategyId { get; }
    public decimal TotalReturnPercent { get; }

    public BacktestCompletedEvent(BacktestId backtestId, StrategyId strategyId, decimal totalReturnPercent, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        BacktestId = backtestId;
        StrategyId = strategyId;
        TotalReturnPercent = totalReturnPercent;
    }
}

public sealed class BacktestFailedEvent : DomainEvent
{
    public BacktestId BacktestId { get; }
    public StrategyId StrategyId { get; }
    public string ErrorMessage { get; }

    public BacktestFailedEvent(BacktestId backtestId, StrategyId strategyId, string errorMessage, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        BacktestId = backtestId;
        StrategyId = strategyId;
        ErrorMessage = errorMessage;
    }
}
