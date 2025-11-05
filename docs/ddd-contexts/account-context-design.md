# Account Context - DDD 상세 설계

## 📋 목차
1. [Context Overview](#1-context-overview)
2. [Domain Model 상세 설계](#2-domain-model-상세-설계)
3. [Business Rules & Invariants](#3-business-rules--invariants)
4. [Domain Events](#4-domain-events)
5. [Application Services](#5-application-services)
6. [Infrastructure Layer](#6-infrastructure-layer)
7. [API/Interface Specifications](#7-apiinterface-specifications)
8. [Error Handling & Validation](#8-error-handling--validation)
9. [External System Integration](#9-external-system-integration)
10. [Performance Considerations](#10-performance-considerations)
11. [Security Considerations](#11-security-considerations)

---

## 1. Context Overview

### 1.1 책임과 범위

**Account Context**는 계좌 관리, 자금 관리, 수익률 추적, 세금 처리 등 모든 계좌 관련 업무를 담당하는 핵심 Bounded Context입니다.

#### 주요 책임
- ✅ 계좌 정보 관리 및 인증
- ✅ 잔고 및 증거금 실시간 관리
- ✅ 예수금/증거금 계산 및 추적
- ✅ 수익률 계산 및 성과 분석
- ✅ 세금 계산 및 보고
- ✅ 다중 계좌 통합 관리
- ✅ 계좌 이력 및 감사 추적
- ✅ 출금 제한 및 자금 이동 관리
- ✅ 마진 관리 및 마진콜 처리
- ✅ 일일 거래 한도 관리

#### 핵심 원칙
- **자금 안전성 최우선**: 모든 거래는 거래 가능 금액 내에서만 진행
- **실시간 잔고 동기화**: 브로커와의 실시간 동기화
- **정확한 수익률 계산**: 수수료, 세금을 반영한 정확한 계산
- **규제 준수**: 세금 법규, 보고서 요건 준수
- **다중 계좌 격리**: 계좌별 독립적 관리 및 보안
- **감사 추적**: 모든 거래 및 변화에 대한 기록 보관

### 1.2 Context Map

```
┌─────────────────────────────────────────────────────────┐
│                    Account Context                      │
│                   (거래 계좌 관리)                       │
└─────────────────────────────────────────────────────────┘
        ↓ 계약                    ↑ 조회
        │                         │
   ┌────┴──────────────┬──────────┴────┐
   ↓                   ↓                ↓
Trading Context   Market Data       Monitoring
(거래 실행)        Context           Context
                  (시세 정보)        (모니터링)
   ↑ 거래 실행           
   │ 결과                
Risk Management  ← 거래 전 검증     ← 포지션 정보
Context
(리스크 관리)
```

### 1.3 바운더리

**이 Context가 담당하는 것:**
- 계좌 데이터 모델 및 저장소
- 잔고 계산 및 업데이트 로직
- 수익률 및 성과 지표 계산
- 출금/입금 관리
- 마진 관리

**이 Context가 담당하지 않는 것:**
- 거래 실행 (Trading Context)
- 시세 데이터 (Market Data Context)
- 리스크 관리 규칙 (Risk Management Context)
- 모니터링 및 알림 (Monitoring Context)

---

## 2. Domain Model 상세 설계

### 2.1 Aggregates

#### 2.1.1 TradingAccount Aggregate (거래 계좌)

**Aggregate Root**

```csharp
/// <summary>
/// 거래 계좌 Aggregate Root
/// 계좌의 모든 정보와 상태를 관리하는 핵심 집합체
/// </summary>
public class TradingAccount : AggregateRoot<AccountId>
{
    private readonly List<AccountBalance> _balances = new();
    private readonly List<AccountRestriction> _restrictions = new();
    private readonly List<PerformanceRecord> _performanceHistory = new();
    private readonly Dictionary<Currency, Money> _multiCurrencyBalances = new();
    private readonly List<WithdrawalRequest> _withdrawalHistory = new();
    
    // ======================== Identity ========================
    /// <summary>계좌 고유 ID</summary>
    public AccountId Id { get; private set; }
    
    /// <summary>브로커 계좌번호 (키움 API)</summary>
    public AccountNumber AccountNumber { get; private set; }
    
    /// <summary>계좌 별칭</summary>
    public AccountName Name { get; private set; }
    
    // ======================== Account Information ========================
    /// <summary>계좌 유형 (현금/신용/선물옵션)</summary>
    public AccountType Type { get; private set; }
    
    /// <summary>계좌 상태 (Active/Suspended/Closed)</summary>
    public AccountStatus Status { get; private set; }
    
    /// <summary>거래 권한</summary>
    public TradingPermission Permission { get; private set; }
    
    /// <summary>계좌 소유자</summary>
    public UserId OwnerId { get; private set; }
    
    /// <summary>브로커 정보</summary>
    public BrokerInfo Broker { get; private set; }
    
    // ======================== Balances (잔고) ========================
    /// <summary>총자산 (현금 + 포지션 평가금액)</summary>
    public Money TotalAssets { get; private set; }
    
    /// <summary>현금 잔고</summary>
    public Money CashBalance { get; private set; }
    
    /// <summary>주문 가능 금액 (거래 가능 현금)</summary>
    public Money AvailableBalance { get; private set; }
    
    /// <summary>포지션 평가 금액</summary>
    public Money TotalEvaluation { get; private set; }
    
    /// <summary>총 손익 (실현 + 미실현)</summary>
    public Money TotalProfitLoss { get; private set; }
    
    /// <summary>실현 손익 (이미 결정된 손익)</summary>
    public Money RealizedPL { get; private set; }
    
    /// <summary>미실현 손익 (현재 포지션 평가 손익)</summary>
    public Money UnrealizedPL { get; private set; }
    
    // ======================== Margin (증거금 관리) ========================
    /// <summary>신용 거래 계좌 정보</summary>
    public MarginAccount Margin { get; private set; }
    
    /// <summary>현재 증거금 잔고</summary>
    public Money MarginBalance { get; private set; }
    
    /// <summary>유지 증거금 (필수 유지해야 할 최소 증거금)</summary>
    public Money MaintenanceMargin { get; private set; }
    
    /// <summary>증거금 비율 (현재증거금/필요증거금)</summary>
    public decimal MarginLevel { get; private set; }
    
    /// <summary>마진콜 발생 시 필요한 추가 증거금</summary>
    public Money MarginCallAmount { get; private set; }
    
    // ======================== Buying Power (매수여력) ========================
    /// <summary>매수 가능 금액</summary>
    public Money BuyingPower { get; private set; }
    
    /// <summary>당일 거래용 매수여력 (데이트레이딩)</summary>
    public Money DayTradingBuyingPower { get; private set; }
    
    /// <summary>야간 매수여력 (익일까지 보유 시 적용)</summary>
    public Money OvernightBuyingPower { get; private set; }
    
    // ======================== Position Summary ========================
    /// <summary>현재 오픈된 포지션 수</summary>
    public int OpenPositionCount { get; private set; }
    
    /// <summary>포지션의 총 평가 금액</summary>
    public Money TotalPositionValue { get; private set; }
    
    /// <summary>현금 비중 (현금 / 총자산)</summary>
    public decimal CashRatio { get; private set; }
    
    /// <summary>포지션 비중 (포지션가치 / 총자산)</summary>
    public decimal PositionRatio { get; private set; }
    
    // ======================== Performance Metrics ========================
    /// <summary>성과 지표 종합</summary>
    public PerformanceMetrics Performance { get; private set; }
    
    /// <summary>당일 손익</summary>
    public Money DailyPL { get; private set; }
    
    /// <summary>주간 손익</summary>
    public Money WeeklyPL { get; private set; }
    
    /// <summary>월간 손익</summary>
    public Money MonthlyPL { get; private set; }
    
    /// <summary>연간 손익</summary>
    public Money YearlyPL { get; private set; }
    
    /// <summary>총 수익률</summary>
    public decimal TotalReturnRate { get; private set; }
    
    // ======================== Risk Metrics ========================
    /// <summary>최대 손실폭 (최고점에서 최저점까지의 낙폭)</summary>
    public Money MaxDrawdown { get; private set; }
    
    /// <summary>샤프 비율 (위험 조정 수익률)</summary>
    public decimal SharpeRatio { get; private set; }
    
    /// <summary>승률 (수익거래 / 전체거래)</summary>
    public decimal WinRate { get; private set; }
    
    /// <summary>수익팩터 (수익거래합 / 손실거래합)</summary>
    public decimal ProfitFactor { get; private set; }
    
    // ======================== Restrictions (제약사항) ========================
    /// <summary>계좌 제약 사항 목록</summary>
    public IReadOnlyList<AccountRestriction> Restrictions => _restrictions.AsReadOnly();
    
    /// <summary>일일 출금 한도</summary>
    public Money DailyWithdrawalLimit { get; private set; }
    
    /// <summary>일일 거래 한도</summary>
    public Money DailyTradingLimit { get; private set; }
    
    /// <summary>패턴 데이트레이더 여부 (미국 규정)</summary>
    public bool IsPatternDayTrader { get; private set; }
    
    // ======================== Tax Information ========================
    /// <summary>세금 프로필</summary>
    public TaxProfile TaxProfile { get; private set; }
    
    /// <summary>연도별 과세 대상 이득</summary>
    public Money YearToDateTaxableGain { get; private set; }
    
    /// <summary>예상 세금 채무</summary>
    public Money EstimatedTaxLiability { get; private set; }
    
    // ======================== Metadata ========================
    /// <summary>계좌 생성 시간</summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>마지막 업데이트 시간</summary>
    public DateTime LastUpdatedAt { get; private set; }
    
    /// <summary>마지막 거래 시간</summary>
    public DateTime LastTradedAt { get; private set; }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 출금 요청
    /// </summary>
    /// <param name="amount">출금 금액</param>
    /// <param name="destination">출금 계좌</param>
    /// <returns>출금 결과</returns>
    /// <exception cref="InsufficientFundsException">거래 가능 금액 부족</exception>
    /// <exception cref="WithdrawalLimitExceededException">일일 출금 한도 초과</exception>
    /// <exception cref="AccountRestrictionException">계좌 제약 위반</exception>
    public WithdrawalResult RequestWithdrawal(Money amount, BankAccount destination)
    {
        // Invariant: 거래 가능 금액 확인
        if (amount > AvailableBalance)
            throw new InsufficientFundsException(
                $"Available: {AvailableBalance}, Requested: {amount}");
        
        // Invariant: 일일 출금 한도 확인
        if (amount > GetDailyWithdrawalRemaining())
            throw new WithdrawalLimitExceededException(
                $"Daily limit exceeded. Max: {DailyWithdrawalLimit}");
        
        // Invariant: 계좌 제약 확인
        if (HasActiveRestriction(RestrictionType.WithdrawalBlock))
            throw new AccountRestrictionException("Withdrawal blocked by restriction");
        
        // 출금 요청 생성
        var withdrawal = new AccountWithdrawal(
            WithdrawalId.New(),
            amount,
            destination,
            DateTime.UtcNow,
            WithdrawalStatus.Pending);
        
        // 잔고 업데이트
        CashBalance -= amount;
        AvailableBalance -= amount;
        
        // 출금 이력 기록
        _withdrawalHistory.Add(withdrawal);
        
        // 도메인 이벤트 발행
        AddDomainEvent(new WithdrawalRequestedEvent(
            Id,
            withdrawal.Id,
            amount,
            destination,
            DateTime.UtcNow));
        
        LastUpdatedAt = DateTime.UtcNow;
        
        return WithdrawalResult.Success(withdrawal);
    }
    
    /// <summary>
    /// 거래 실행으로 인한 잔고 업데이트
    /// </summary>
    /// <param name="execution">거래 실행 정보</param>
    public void UpdateBalanceFromExecution(OrderExecution execution)
    {
        var executionValue = execution.Price * execution.Quantity;
        var commission = CalculateCommission(executionValue);
        var tax = CalculateTax(execution);
        var totalCost = executionValue + commission + tax;
        
        if (execution.Side == OrderSide.Buy)
        {
            // 매수: 현금 감소
            if (totalCost > AvailableBalance)
                throw new InsufficientFundsException(
                    $"Insufficient balance for buy order. Cost: {totalCost}, Available: {AvailableBalance}");
            
            CashBalance -= totalCost;
            AvailableBalance = CalculateAvailableBalance();
        }
        else if (execution.Side == OrderSide.Sell)
        {
            // 매도: 현금 증가
            var proceeds = executionValue - commission - tax;
            CashBalance += proceeds;
            AvailableBalance = CalculateAvailableBalance();
            
            // 실현 손익 계산 및 세금 정보 업데이트
            var realizedPL = CalculateRealizedPL(execution);
            RealizedPL += realizedPL;
            
            // 양수 이익일 경우 세금 정보 업데이트
            if (realizedPL > 0)
            {
                YearToDateTaxableGain += realizedPL;
                EstimatedTaxLiability = CalculateEstimatedTax();
            }
        }
        
        // 메타데이터 업데이트
        LastTradedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
        
        // 도메인 이벤트 발행
        AddDomainEvent(new AccountBalanceUpdatedEvent(
            Id,
            CashBalance,
            AvailableBalance,
            execution,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// 포지션 크기 검증
    /// </summary>
    /// <param name="positionValue">포지션 금액</param>
    /// <param name="stockCode">종목 코드</param>
    /// <returns>검증 결과</returns>
    public PositionSizeValidation ValidatePositionSize(Money positionValue, StockCode stockCode)
    {
        var validation = new PositionSizeValidation();
        
        // 검증 1: 매수 여력 확인
        if (positionValue > BuyingPower)
        {
            validation.AddError(
                $"Insufficient buying power. Available: {BuyingPower}, Required: {positionValue}");
        }
        
        // 검증 2: 집중 위험도 확인 (단일 종목 비중 제한 30%)
        var maxConcentration = TotalAssets * 0.3m;
        if (positionValue > maxConcentration)
        {
            validation.AddWarning(
                $"Position concentration exceeds 30% limit. Max: {maxConcentration}");
        }
        
        // 검증 3: 데이트레이딩 규칙 확인 (미국 PDT)
        if (IsPatternDayTrader && DayTradingBuyingPower < positionValue)
        {
            validation.AddError(
                "Pattern day trader buying power exceeded. Max day trades: 3 per 5 days");
        }
        
        // 검증 4: 증거금 확인 (신용거래 계좌)
        if (Type == AccountType.Margin)
        {
            var marginRequirement = GetMarginRequirement(stockCode);
            var requiredMargin = positionValue * marginRequirement;
            
            if (requiredMargin > MarginBalance)
            {
                validation.AddError(
                    $"Insufficient margin. Required: {requiredMargin}, Available: {MarginBalance}");
            }
        }
        
        return validation;
    }
    
    /// <summary>
    /// 성과 지표 재계산
    /// </summary>
    /// <param name="positions">현재 포지션 목록</param>
    /// <param name="trades">거래 기록</param>
    /// <param name="currentPrices">현재 시세</param>
    public void RecalculatePerformanceMetrics(
        IEnumerable<Position> positions,
        IEnumerable<Trade> trades,
        MarketData currentPrices)
    {
        var positionList = positions.ToList();
        var tradeList = trades.ToList();
        
        // 1. 미실현 손익 계산
        UnrealizedPL = positionList.Sum(p =>
            p.CalculateUnrealizedPL(currentPrices.GetPrice(p.StockCode)));
        
        // 2. 총 손익 업데이트 (실현 + 미실현)
        TotalProfitLoss = RealizedPL + UnrealizedPL;
        
        // 3. 총 수익률 계산
        var initialCapital = CalculateInitialCapital();
        TotalReturnRate = initialCapital > 0 ? TotalProfitLoss / initialCapital : 0;
        
        // 4. 기간별 손익 계산
        DailyPL = CalculatePeriodPL(tradeList, TimeSpan.FromDays(1));
        WeeklyPL = CalculatePeriodPL(tradeList, TimeSpan.FromDays(7));
        MonthlyPL = CalculatePeriodPL(tradeList, TimeSpan.FromDays(30));
        YearlyPL = CalculatePeriodPL(tradeList, TimeSpan.FromDays(365));
        
        // 5. 위험 조정 성과 지표 계산
        Performance = new PerformanceMetrics
        {
            TotalReturn = TotalReturnRate,
            DailyReturn = DailyPL / initialCapital,
            WeeklyReturn = WeeklyPL / initialCapital,
            MonthlyReturn = MonthlyPL / initialCapital,
            YearlyReturn = YearlyPL / initialCapital,
            SharpeRatio = CalculateSharpeRatio(tradeList),
            MaxDrawdown = CalculateMaxDrawdown(_performanceHistory),
            WinRate = CalculateWinRate(tradeList),
            ProfitFactor = CalculateProfitFactor(tradeList),
            AverageWin = CalculateAverageWin(tradeList),
            AverageLoss = CalculateAverageLoss(tradeList),
            RecoveryFactor = CalculateRecoveryFactor(tradeList),
            ConcentrationRisk = CalculateConcentrationRisk(positionList),
            DiversificationScore = CalculateDiversificationScore(positionList)
        };
        
        // 6. 위치 정보 업데이트
        OpenPositionCount = positionList.Count;
        TotalPositionValue = positionList.Sum(p => p.CurrentValue);
        CashRatio = TotalAssets > 0 ? CashBalance / TotalAssets : 1;
        PositionRatio = TotalAssets > 0 ? TotalPositionValue / TotalAssets : 0;
        
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 마진콜 확인
    /// </summary>
    /// <returns>마진콜 여부</returns>
    public bool CheckMarginCall()
    {
        if (Type != AccountType.Margin)
            return false;
        
        MarginLevel = MaintenanceMargin > 0
            ? MarginBalance / MaintenanceMargin
            : 1m;
        
        // 마진 비율이 최소 유지 기준 이하이면 마진콜 발생
        if (MarginLevel < 1m)
        {
            MarginCallAmount = MaintenanceMargin - MarginBalance;
            
            AddDomainEvent(new MarginCallTriggeredEvent(
                Id,
                MaintenanceMargin,
                MarginBalance,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow));
            
            return true;
        }
        
        MarginCallAmount = Money.Zero;
        return false;
    }
    
    /// <summary>
    /// 브로커 데이터와 동기화
    /// </summary>
    /// <param name="brokerBalance">브로커로부터 받은 잔고 정보</param>
    /// <param name="brokerPositions">브로커로부터 받은 포지션 정보</param>
    public void SyncWithBrokerData(BrokerAccountBalance brokerBalance, BrokerPosition[] brokerPositions)
    {
        // 현금 잔고 동기화
        if (CashBalance != brokerBalance.CashBalance)
        {
            var difference = brokerBalance.CashBalance - CashBalance;
            CashBalance = brokerBalance.CashBalance;
            
            AddDomainEvent(new BrokerSyncEvent(
                Id,
                "CashBalance",
                CashBalance,
                difference,
                DateTime.UtcNow));
        }
        
        // 증거금 동기화
        if (Type == AccountType.Margin && Margin != null)
        {
            Margin.Sync(brokerBalance.MarginBalance, brokerBalance.MaintenanceMargin);
        }
        
        // 메타데이터 업데이트
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    // ======================== Private Helper Methods ========================
    
    private Money GetDailyWithdrawalRemaining()
    {
        var todayWithdrawals = _withdrawalHistory
            .Where(w => w.CreatedAt.Date == DateTime.UtcNow.Date && w.Status == WithdrawalStatus.Completed)
            .Sum(w => w.Amount);
        
        return DailyWithdrawalLimit - todayWithdrawals;
    }
    
    private bool HasActiveRestriction(RestrictionType type)
    {
        return _restrictions.Any(r =>
            r.Type == type &&
            r.Status == RestrictionStatus.Active &&
            r.ExpiresAt > DateTime.UtcNow);
    }
    
    private Money CalculateAvailableBalance()
    {
        // 주문가능금액 = 현금잔고 - 미체결주문금액
        return CashBalance;
    }
    
    private Money CalculateCommission(Money executionValue)
    {
        // 수수료 계산: 거래금액의 0.05%
        return executionValue * 0.0005m;
    }
    
    private Money CalculateTax(OrderExecution execution)
    {
        if (execution.Side == OrderSide.Buy)
            return Money.Zero; // 매수 시 세금 없음
        
        // 매도 시 거래세: 거래금액의 0.003%
        return (execution.Price * execution.Quantity) * 0.00003m;
    }
    
    private Money CalculateRealizedPL(OrderExecution execution)
    {
        // 실현 손익은 Trading Context에서 계산되어 온다고 가정
        return execution.ProfitLoss;
    }
    
    private Money CalculateEstimatedTax()
    {
        // 양도소득세 계산 (20% 일반)
        return YearToDateTaxableGain * 0.20m;
    }
    
    private Money CalculatePeriodPL(List<Trade> trades, TimeSpan period)
    {
        var startDate = DateTime.UtcNow.Subtract(period);
        return trades
            .Where(t => t.ClosedAt.HasValue && t.ClosedAt.Value > startDate)
            .Sum(t => t.ProfitLoss);
    }
    
    private decimal CalculateSharpeRatio(List<Trade> trades)
    {
        if (trades.Count < 2) return 0;
        
        var returns = trades.Select(t => t.ProfitLoss.Amount / (decimal)1000000).ToList();
        var avgReturn = returns.Average();
        var variance = returns.Sum(r => (r - avgReturn) * (r - avgReturn)) / returns.Count;
        var stdDev = (decimal)Math.Sqrt((double)variance);
        
        return stdDev > 0 ? (avgReturn / stdDev) * (decimal)Math.Sqrt(252) : 0; // 252 trading days
    }
    
    private Money CalculateMaxDrawdown(List<PerformanceRecord> history)
    {
        if (!history.Any()) return Money.Zero;
        
        var peak = Money.Zero;
        var maxDD = Money.Zero;
        
        foreach (var record in history.OrderBy(r => r.Date))
        {
            if (record.EquityValue > peak)
                peak = record.EquityValue;
            
            var drawdown = peak - record.EquityValue;
            if (drawdown > maxDD)
                maxDD = drawdown;
        }
        
        return maxDD;
    }
    
    private decimal CalculateWinRate(List<Trade> trades)
    {
        if (trades.Count == 0) return 0;
        
        var winningTrades = trades.Count(t => t.ProfitLoss > 0);
        return (decimal)winningTrades / trades.Count;
    }
    
    private decimal CalculateProfitFactor(List<Trade> trades)
    {
        if (trades.Count == 0) return 0;
        
        var totalWins = trades.Where(t => t.ProfitLoss > 0).Sum(t => t.ProfitLoss.Amount);
        var totalLosses = Math.Abs(trades.Where(t => t.ProfitLoss < 0).Sum(t => t.ProfitLoss.Amount));
        
        return totalLosses > 0 ? totalWins / totalLosses : (totalWins > 0 ? decimal.MaxValue : 0);
    }
    
    private Money CalculateAverageWin(List<Trade> trades)
    {
        var winningTrades = trades.Where(t => t.ProfitLoss > 0).ToList();
        if (!winningTrades.Any()) return Money.Zero;
        
        return new Money(winningTrades.Average(t => t.ProfitLoss.Amount));
    }
    
    private Money CalculateAverageLoss(List<Trade> trades)
    {
        var losingTrades = trades.Where(t => t.ProfitLoss < 0).ToList();
        if (!losingTrades.Any()) return Money.Zero;
        
        return new Money(losingTrades.Average(t => t.ProfitLoss.Amount));
    }
    
    private decimal CalculateRecoveryFactor(List<Trade> trades)
    {
        var totalPL = trades.Sum(t => t.ProfitLoss.Amount);
        var maxDD = CalculateMaxDrawdown(_performanceHistory);
        
        return maxDD.Amount > 0 ? totalPL / maxDD.Amount : 0;
    }
    
    private decimal CalculateConcentrationRisk(List<Position> positions)
    {
        if (!positions.Any() || TotalPositionValue <= 0)
            return 0;
        
        // 허쉬만-허핀달 지수 (HHI)
        return positions.Sum(p => (decimal)Math.Pow((double)(p.CurrentValue / TotalPositionValue), 2));
    }
    
    private decimal CalculateDiversificationScore(List<Position> positions)
    {
        // 포지션 수가 많고 집중도가 낮을수록 높음 (0~1)
        var positionCount = positions.Count;
        var concentration = CalculateConcentrationRisk(positions);
        
        return Math.Min(1, (decimal)(1 - concentration) * (Math.Min(10, positionCount) / 10m));
    }
    
    private Money CalculateInitialCapital()
    {
        // 초기 자본 = 최초 입금액
        // 실제로는 DB에서 조회하거나 별도 필드에서 가져옴
        return TotalAssets - TotalProfitLoss;
    }
    
    private decimal GetMarginRequirement(StockCode stockCode)
    {
        // 브로커 규정에 따른 증거금 요구율 (기본 40%)
        return 0.4m;
    }
}
```

### 2.2 Value Objects

```csharp
/// <summary>계좌 ID (고유 식별자)</summary>
public sealed class AccountId : StronglyTypedId<Guid>
{
    public AccountId(Guid value) : base(value) { }
    public static AccountId New() => new(Guid.NewGuid());
}

/// <summary>계좌번호 (브로커 계좌번호)</summary>
public sealed class AccountNumber : ValueObject
{
    public string Value { get; }
    
    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account number cannot be empty");
        if (!value.All(c => char.IsDigit(c)))
            throw new ArgumentException("Account number must contain only digits");
        
        Value = value;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

/// <summary>계좌명</summary>
public sealed class AccountName : ValueObject
{
    public string Value { get; }
    
    public AccountName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 50)
            throw new ArgumentException("Account name must be between 1 and 50 characters");
        
        Value = value;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

/// <summary>돈 (Money) - 통화와 금액을 함께 관리</summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    public decimal Amount { get; }
    public Currency Currency { get; }
    
    public Money(decimal amount, Currency currency = Currency.KRW)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public Money(decimal amount) : this(amount, Currency.KRW) { }
    
    public static Money Zero => new(0);
    
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money in different currencies");
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money in different currencies");
        return new Money(left.Amount - right.Amount, left.Currency);
    }
    
    public static Money operator *(Money money, decimal multiplier)
        => new(money.Amount * multiplier, money.Currency);
    
    public static Money operator /(Money money, decimal divisor)
        => new(money.Amount / divisor, money.Currency);
    
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;
    
    public int CompareTo(Money other)
    {
        if (other is null) return 1;
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money in different currencies");
        return Amount.CompareTo(other.Amount);
    }
    
    public override string ToString() => $"{Amount:N0} {Currency}";
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

/// <summary>성과 지표</summary>
public sealed class PerformanceMetrics : ValueObject
{
    public decimal TotalReturn { get; set; }
    public decimal DailyReturn { get; set; }
    public decimal WeeklyReturn { get; set; }
    public decimal MonthlyReturn { get; set; }
    public decimal YearlyReturn { get; set; }
    public decimal SharpeRatio { get; set; }
    public Money MaxDrawdown { get; set; }
    public decimal WinRate { get; set; }
    public decimal ProfitFactor { get; set; }
    public Money AverageWin { get; set; }
    public Money AverageLoss { get; set; }
    public decimal RecoveryFactor { get; set; }
    public decimal ConcentrationRisk { get; set; }
    public decimal DiversificationScore { get; set; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalReturn;
        yield return SharpeRatio;
        yield return WinRate;
        yield return MaxDrawdown;
    }
}

/// <summary>거래 권한</summary>
public sealed class TradingPermission : ValueObject
{
    public bool CanBuy { get; }
    public bool CanSell { get; }
    public bool CanShort { get; }
    public bool CanOptions { get; }
    public bool CanFutures { get; }
    
    public TradingPermission(
        bool canBuy = true,
        bool canSell = true,
        bool canShort = false,
        bool canOptions = false,
        bool canFutures = false)
    {
        CanBuy = canBuy;
        CanSell = canSell;
        CanShort = canShort;
        CanOptions = canOptions;
        CanFutures = canFutures;
    }
    
    public bool IsPermitted(OrderSide side, SecurityType securityType)
    {
        return (side, securityType) switch
        {
            (OrderSide.Buy, SecurityType.Stock) => CanBuy,
            (OrderSide.Sell, SecurityType.Stock) => CanSell,
            (OrderSide.Short, SecurityType.Stock) => CanShort,
            (_, SecurityType.Option) => CanOptions,
            (_, SecurityType.Futures) => CanFutures,
            _ => false
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CanBuy;
        yield return CanSell;
        yield return CanShort;
        yield return CanOptions;
        yield return CanFutures;
    }
}

/// <summary>세금 프로필</summary>
public sealed class TaxProfile : ValueObject
{
    public TaxResidentType ResidentType { get; } // 거주자/비거주자
    public decimal TaxRate { get; } // 세율 (20%, 33% 등)
    public DateTime TaxYearStart { get; } // 과세연도 시작
    public DateTime TaxYearEnd { get; } // 과세연도 끝
    
    public TaxProfile(
        TaxResidentType residentType,
        decimal taxRate,
        DateTime taxYearStart,
        DateTime taxYearEnd)
    {
        ResidentType = residentType;
        TaxRate = taxRate;
        TaxYearStart = taxYearStart;
        TaxYearEnd = taxYearEnd;
    }
    
    public bool IsWithinTaxYear(DateTime date)
        => date >= TaxYearStart && date <= TaxYearEnd;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ResidentType;
        yield return TaxRate;
        yield return TaxYearStart;
        yield return TaxYearEnd;
    }
}

/// <summary>마진 계좌</summary>
public sealed class MarginAccount : ValueObject
{
    public Money MarginBalance { get; private set; } // 증거금 잔고
    public Money MaintenanceMargin { get; private set; } // 유지 증거금
    public decimal MarginMultiplier { get; } // 증거금 배수 (2x, 3x 등)
    public DateTime EnabledAt { get; }
    
    public MarginAccount(
        Money initialMargin,
        decimal marginMultiplier = 2m)
    {
        MarginBalance = initialMargin;
        MaintenanceMargin = initialMargin * 0.5m; // 유지 증거금 = 50%
        MarginMultiplier = marginMultiplier;
        EnabledAt = DateTime.UtcNow;
    }
    
    public void UpdateMarginBalances(Money newBalance, Money newMaintenance)
    {
        MarginBalance = newBalance;
        MaintenanceMargin = newMaintenance;
    }
    
    public bool IsMarginCallActive()
        => MarginBalance < MaintenanceMargin;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MarginBalance;
        yield return MaintenanceMargin;
        yield return MarginMultiplier;
    }
}

/// <summary>브로커 정보</summary>
public sealed class BrokerInfo : ValueObject
{
    public string BrokerName { get; } // ex: "Kiwoom"
    public string BrokerApiVersion { get; }
    public DateTime ConnectedAt { get; }
    
    public BrokerInfo(
        string brokerName,
        string brokerApiVersion)
    {
        BrokerName = brokerName;
        BrokerApiVersion = brokerApiVersion;
        ConnectedAt = DateTime.UtcNow;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BrokerName;
        yield return BrokerApiVersion;
    }
}
```

### 2.3 Entities

```csharp
/// <summary>계좌 잔고 히스토리</summary>
public class AccountBalance : Entity<AccountBalanceId>
{
    public AccountId AccountId { get; init; }
    public DateTime BalanceDate { get; init; }
    public Money CashBalance { get; init; }
    public Money TotalAssets { get; init; }
    public Money TotalEvaluation { get; init; }
    public Money TotalPL { get; init; }
    public DateTime RecordedAt { get; init; }
}

/// <summary>성과 기록</summary>
public class PerformanceRecord : Entity<PerformanceRecordId>
{
    public AccountId AccountId { get; init; }
    public DateTime Date { get; init; }
    public Money EquityValue { get; init; }
    public Money DailyReturn { get; init; }
    public decimal DailyReturnPercent { get; init; }
}

/// <summary>계좌 제약</summary>
public class AccountRestriction : Entity<AccountRestrictionId>
{
    public AccountId AccountId { get; init; }
    public RestrictionType Type { get; init; }
    public RestrictionStatus Status { get; init; }
    public string Reason { get; init; }
    public DateTime AppliedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string AppliedBy { get; init; }
}

/// <summary>출금 요청</summary>
public class AccountWithdrawal : Entity<WithdrawalId>
{
    public AccountId AccountId { get; init; }
    public Money Amount { get; init; }
    public BankAccount Destination { get; init; }
    public WithdrawalStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string FailureReason { get; init; }
}

/// <summary>출금 계좌</summary>
public sealed class BankAccount : ValueObject
{
    public string BankCode { get; }
    public string AccountNumber { get; }
    public string AccountHolder { get; }
    
    public BankAccount(string bankCode, string accountNumber, string accountHolder)
    {
        BankCode = bankCode ?? throw new ArgumentNullException(nameof(bankCode));
        AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        AccountHolder = accountHolder ?? throw new ArgumentNullException(nameof(accountHolder));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BankCode;
        yield return AccountNumber;
    }
}
```

### 2.4 Enumerations

```csharp
/// <summary>계좌 유형</summary>
public enum AccountType
{
    Cash = 1,          // 현금
    Margin = 2,        // 신용거래
    Futures = 3,       // 선물옵션
    DayTrading = 4     // 데이트레이딩
}

/// <summary>계좌 상태</summary>
public enum AccountStatus
{
    Active = 1,        // 활성
    Suspended = 2,     // 정지
    Closed = 3,        // 폐쇄
    Inactive = 4       // 휴면
}

/// <summary>주문 방향</summary>
public enum OrderSide
{
    Buy = 1,
    Sell = 2,
    Short = 3
}

/// <summary>증권 종류</summary>
public enum SecurityType
{
    Stock = 1,
    ETF = 2,
    Option = 3,
    Futures = 4
}

/// <summary>통화</summary>
public enum Currency
{
    KRW = 1,
    USD = 2,
    JPY = 3
}

/// <summary>제약 유형</summary>
public enum RestrictionType
{
    WithdrawalBlock = 1,     // 출금 금지
    TradingBlock = 2,        // 거래 금지
    ShortSellingBlock = 3    // 공매도 금지
}

/// <summary>제약 상태</summary>
public enum RestrictionStatus
{
    Active = 1,
    Expired = 2,
    Removed = 3
}

/// <summary>출금 상태</summary>
public enum WithdrawalStatus
{
    Pending = 1,       // 대기
    Processing = 2,    // 처리 중
    Completed = 3,     // 완료
    Failed = 4,        // 실패
    Cancelled = 5      // 취소
}

/// <summary>세금 거주자 유형</summary>
public enum TaxResidentType
{
    Resident = 1,      // 거주자
    NonResident = 2    // 비거주자
}
```

---

## 3. Business Rules & Invariants

### 3.1 핵심 불변식 (Invariants)

```csharp
/// <summary>
/// 계좌의 핵심 비즈니스 규칙을 검증하는 클래스
/// </summary>
public static class AccountInvariants
{
    /// <summary>
    /// Invariant 1: 총자산 = 현금 + 포지션 평가금액
    /// </summary>
    public static void ValidateTotalAssets(TradingAccount account, Money portfolioValue)
    {
        var expectedTotal = account.CashBalance + portfolioValue;
        
        if (Math.Abs((account.TotalAssets - expectedTotal).Amount) > 1)
        {
            throw new InvalidOperationException(
                $"Total assets mismatch. Expected: {expectedTotal}, Actual: {account.TotalAssets}");
        }
    }
    
    /// <summary>
    /// Invariant 2: 주문 가능 금액 <= 현금 잔고
    /// </summary>
    public static void ValidateAvailableBalance(TradingAccount account)
    {
        if (account.AvailableBalance > account.CashBalance)
        {
            throw new InvalidOperationException(
                "Available balance cannot exceed cash balance");
        }
    }
    
    /// <summary>
    /// Invariant 3: 거래 가능 금액은 음수가 될 수 없음
    /// </summary>
    public static void ValidateBuyingPower(TradingAccount account)
    {
        if (account.BuyingPower.Amount < 0)
        {
            throw new InvalidOperationException(
                "Buying power cannot be negative");
        }
    }
    
    /// <summary>
    /// Invariant 4: 미실현 손익 + 실현 손익 = 총 손익
    /// </summary>
    public static void ValidateProfitLoss(TradingAccount account)
    {
        var expected = account.RealizedPL + account.UnrealizedPL;
        
        if (Math.Abs((account.TotalProfitLoss - expected).Amount) > 1)
        {
            throw new InvalidOperationException(
                "Total P&L mismatch");
        }
    }
    
    /// <summary>
    /// Invariant 5: 신용거래 계좌의 증거금은 유지 증거금 이상이어야 함
    /// </summary>
    public static void ValidateMarginRequirement(TradingAccount account)
    {
        if (account.Type != AccountType.Margin)
            return;
        
        if (account.MarginBalance < account.MaintenanceMargin)
        {
            throw new InvalidOperationException(
                "Margin balance below maintenance requirement (Margin Call)");
        }
    }
    
    /// <summary>
    /// Invariant 6: 수익률은 -100% 이상 300% 이하
    /// </summary>
    public static void ValidateReturnRate(TradingAccount account)
    {
        if (account.TotalReturnRate < -1m || account.TotalReturnRate > 3m)
        {
            throw new InvalidOperationException(
                "Return rate out of acceptable range");
        }
    }
    
    /// <summary>
    /// Invariant 7: 출금 불가 상태에서는 출금을 할 수 없음
    /// </summary>
    public static void ValidateWithdrawalAllowed(TradingAccount account)
    {
        if (account.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException(
                $"Cannot withdraw from account in {account.Status} status");
        }
        
        if (account.HasActiveRestriction(RestrictionType.WithdrawalBlock))
        {
            throw new AccountRestrictionException(
                "Account has withdrawal restriction");
        }
    }
}
```

### 3.2 비즈니스 규칙

```csharp
/// <summary>
/// 계좌 관련 비즈니스 규칙
/// </summary>
public static class AccountBusinessRules
{
    /// <summary>
    /// 규칙 1: 일일 출금 한도 확인
    /// 한 계좌에서 하루에 출금할 수 있는 최대 금액
    /// </summary>
    public static bool IsWithinDailyWithdrawalLimit(
        TradingAccount account,
        Money requestedAmount)
    {
        var remaining = account.GetDailyWithdrawalRemaining();
        return requestedAmount <= remaining;
    }
    
    /// <summary>
    /// 규칙 2: 포지션 집중도 확인
    /// 단일 종목이 전체 자산의 30% 이상이 되지 않도록
    /// </summary>
    public static bool IsConcentrationAcceptable(
        TradingAccount account,
        Money newPositionValue)
    {
        var maxConcentration = account.TotalAssets * 0.3m;
        return newPositionValue <= maxConcentration;
    }
    
    /// <summary>
    /// 규칙 3: 패턴 데이트레이더 (PDT) 규칙
    /// 5거래일 중 3회 이상 당일매매를 하면 PDT 지정
    /// </summary>
    public static bool CheckPatternDayTrader(List<Trade> recentTrades)
    {
        var last5Days = DateTime.UtcNow.AddDays(-5);
        var dayTrades = recentTrades
            .Where(t => t.OpenedAt >= last5Days && t.ClosedAt.HasValue && 
                       t.ClosedAt.Value.Date == t.OpenedAt.Date)
            .Count();
        
        return dayTrades >= 3;
    }
    
    /// <summary>
    /// 규칙 4: 증거금 요구율 계산
    /// 종목별로 서로 다른 증거금 요구율 적용
    /// </summary>
    public static decimal GetMarginRequirement(
        StockCode stockCode,
        IMarginPolicyProvider policyProvider)
    {
        var policy = policyProvider.GetPolicy(stockCode);
        return policy?.MarginRequirement ?? 0.4m; // 기본 40%
    }
    
    /// <summary>
    /// 규칙 5: 거래 가능 시간 확인
    /// 일반적으로 평일 09:00 ~ 15:30
    /// </summary>
    public static bool IsTradingHour(DateTime dateTime)
    {
        var hour = dateTime.Hour;
        var minute = dateTime.Minute;
        var dayOfWeek = dateTime.DayOfWeek;
        
        // 주말 제외
        if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            return false;
        
        // 거래 시간: 09:00 ~ 15:30
        return (hour > 9 || (hour == 9 && minute >= 0)) &&
               (hour < 15 || (hour == 15 && minute <= 30));
    }
    
    /// <summary>
    /// 규칙 6: 세금 계산 (양도소득세)
    /// 보유 기간에 따라 다른 세율 적용
    /// </summary>
    public static decimal CalculateTaxRate(
        DateTime purchaseDate,
        DateTime saleDate,
        TaxProfile taxProfile)
    {
        var holdingDays = (saleDate - purchaseDate).TotalDays;
        
        return holdingDays >= 365
            ? taxProfile.TaxRate * 0.5m    // 장기: 50% 감면
            : taxProfile.TaxRate;           // 단기: 정상 세율
    }
    
    /// <summary>
    /// 규칙 7: 거래 수수료 계산
    /// </summary>
    public static Money CalculateCommission(
        Money executionValue,
        SecurityType securityType,
        ICommissionPolicyProvider policyProvider)
    {
        var policy = policyProvider.GetPolicy(securityType);
        var rate = policy?.CommissionRate ?? 0.0005m; // 기본 0.05%
        
        return executionValue * rate;
    }
}
```

---

## 4. Domain Events

```csharp
/// <summary>
/// 잔고 업데이트 이벤트
/// 거래 실행 후 계좌 잔고가 변경되었음을 알림
/// </summary>
public sealed class AccountBalanceUpdatedEvent : DomainEvent
{
    public AccountId AccountId { get; }
    public Money CashBalance { get; }
    public Money AvailableBalance { get; }
    public OrderExecution TriggeringExecution { get; }
    public DateTime UpdatedAt { get; }
    
    public AccountBalanceUpdatedEvent(
        AccountId accountId,
        Money cashBalance,
        Money availableBalance,
        OrderExecution triggeringExecution,
        DateTime updatedAt)
    {
        AccountId = accountId;
        CashBalance = cashBalance;
        AvailableBalance = availableBalance;
        TriggeringExecution = triggeringExecution;
        UpdatedAt = updatedAt;
    }
}

/// <summary>
/// 마진콜 이벤트
/// 증거금이 유지 기준 이하로 떨어짐
/// </summary>
public sealed class MarginCallTriggeredEvent : DomainEvent
{
    public AccountId AccountId { get; }
    public Money RequiredMargin { get; }
    public Money CurrentMargin { get; }
    public DateTime Deadline { get; }
    public DateTime TriggeredAt { get; }
    
    public MarginCallTriggeredEvent(
        AccountId accountId,
        Money requiredMargin,
        Money currentMargin,
        DateTime deadline,
        DateTime triggeredAt)
    {
        AccountId = accountId;
        RequiredMargin = requiredMargin;
        CurrentMargin = currentMargin;
        Deadline = deadline;
        TriggeredAt = triggeredAt;
    }
}

/// <summary>
/// 출금 요청 이벤트
/// 사용자가 출금을 요청함
/// </summary>
public sealed class WithdrawalRequestedEvent : DomainEvent
{
    public AccountId AccountId { get; }
    public WithdrawalId WithdrawalId { get; }
    public Money Amount { get; }
    public BankAccount Destination { get; }
    public DateTime RequestedAt { get; }
    
    public WithdrawalRequestedEvent(
        AccountId accountId,
        WithdrawalId withdrawalId,
        Money amount,
        BankAccount destination,
        DateTime requestedAt)
    {
        AccountId = accountId;
        WithdrawalId = withdrawalId;
        Amount = amount;
        Destination = destination;
        RequestedAt = requestedAt;
    }
}

/// <summary>
/// 성과 지표 업데이트 이벤트
/// 성과 지표가 재계산되어 업데이트됨
/// </summary>
public sealed class PerformanceMetricsUpdatedEvent : DomainEvent
{
    public AccountId AccountId { get; }
    public PerformanceMetrics Metrics { get; }
    public DateTime CalculatedAt { get; }
    
    public PerformanceMetricsUpdatedEvent(
        AccountId accountId,
        PerformanceMetrics metrics,
        DateTime calculatedAt)
    {
        AccountId = accountId;
        Metrics = metrics;
        CalculatedAt = calculatedAt;
    }
}

/// <summary>
/// 브로커 동기화 이벤트
/// 계좌 정보가 브로커와 동기화됨
/// </summary>
public sealed class BrokerSyncEvent : DomainEvent
{
    public AccountId AccountId { get; }
    public string FieldName { get; }
    public object NewValue { get; }
    public object Difference { get; }
    public DateTime SyncedAt { get; }
    
    public BrokerSyncEvent(
        AccountId accountId,
        string fieldName,
        object newValue,
        object difference,
        DateTime syncedAt)
    {
        AccountId = accountId;
        FieldName = fieldName;
        NewValue = newValue;
        Difference = difference;
        SyncedAt = syncedAt;
    }
}

/// <summary>
/// 계좌 제약 적용 이벤트
/// 계좌에 제약이 적용됨
/// </summary>
public sealed class AccountRestrictionAppliedEvent : DomainEvent
{
    public AccountId AccountId { get; }
    public RestrictionType Type { get; }
    public string Reason { get; }
    public DateTime ExpiresAt { get; }
    public DateTime AppliedAt { get; }
    
    public AccountRestrictionAppliedEvent(
        AccountId accountId,
        RestrictionType type,
        string reason,
        DateTime expiresAt,
        DateTime appliedAt)
    {
        AccountId = accountId;
        Type = type;
        Reason = reason;
        ExpiresAt = expiresAt;
        AppliedAt = appliedAt;
    }
}

/// <summary>
/// 세금 계산 이벤트
/// 세금 정보가 업데이트됨
/// </summary>
public sealed class TaxCalculatedEvent : DomainEvent
{
    public AccountId AccountId { get; }
    public Money TaxableGain { get; }
    public Money TaxLiability { get; }
    public DateTime CalculatedAt { get; }
    
    public TaxCalculatedEvent(
        AccountId accountId,
        Money taxableGain,
        Money taxLiability,
        DateTime calculatedAt)
    {
        AccountId = accountId;
        TaxableGain = taxableGain;
        TaxLiability = taxLiability;
        CalculatedAt = calculatedAt;
    }
}
```

---

## 5. Application Services

```csharp
/// <summary>
/// 계좌 애플리케이션 서비스
/// 계좌 관리 관련 비즈니스 프로세스 오케스트레이션
/// </summary>
public sealed class AccountService : IApplicationService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountCalculationService _calculationService;
    private readonly IKiwoomApiService _kiwoomApi;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountService> _logger;
    
    public AccountService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IAccountCalculationService calculationService,
        IKiwoomApiService kiwoomApi,
        IEventBus eventBus,
        IUnitOfWork unitOfWork,
        ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _calculationService = calculationService;
        _kiwoomApi = kiwoomApi;
        _eventBus = eventBus;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    /// <summary>
    /// 계좌 요약 정보 조회
    /// </summary>
    public async Task<AccountSummaryDto> GetAccountSummaryAsync(AccountId accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
            throw new AccountNotFoundException($"Account {accountId} not found");
        
        return new AccountSummaryDto
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber.Value,
            Name = account.Name.Value,
            Type = account.Type.ToString(),
            Status = account.Status.ToString(),
            TotalAssets = account.TotalAssets,
            CashBalance = account.CashBalance,
            AvailableBalance = account.AvailableBalance,
            TotalEvaluation = account.TotalEvaluation,
            TotalProfitLoss = account.TotalProfitLoss,
            TotalReturnRate = account.TotalReturnRate,
            BuyingPower = account.BuyingPower,
            DailyPL = account.DailyPL,
            MarginLevel = account.MarginLevel,
            Performance = account.Performance
        };
    }
    
    /// <summary>
    /// 브로커와 계좌 정보 동기화
    /// </summary>
    public async Task SyncAccountWithBrokerAsync(AccountId accountId)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new AccountNotFoundException($"Account {accountId} not found");
            
            _logger.LogInformation($"Syncing account {account.AccountNumber} with broker");
            
            // 브로커로부터 계좌 잔고 정보 조회
            var kiwoomBalance = await _kiwoomApi.GetAccountBalanceAsync(
                account.AccountNumber.Value);
            
            var kiwoomPositions = await _kiwoomApi.GetPositionsAsync(
                account.AccountNumber.Value);
            
            // 계좌 동기화
            var brokerBalance = new BrokerAccountBalance(
                kiwoomBalance.CashBalance,
                kiwoomBalance.TotalAssets,
                kiwoomBalance.MarginBalance,
                kiwoomBalance.MaintenanceMargin);
            
            account.SyncWithBrokerData(brokerBalance, kiwoomPositions);
            
            // 저장
            await _accountRepository.UpdateAsync(account);
            await _unitOfWork.CommitAsync();
            
            // 이벤트 발행
            foreach (var @event in account.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            _logger.LogInformation($"Account {account.AccountNumber} synced successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error syncing account: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 출금 요청 처리
    /// </summary>
    public async Task<WithdrawalResultDto> ProcessWithdrawalAsync(
        WithdrawalCommand command)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(command.AccountId);
            if (account == null)
                throw new AccountNotFoundException($"Account {command.AccountId} not found");
            
            // 출금 검증 및 처리
            var result = account.RequestWithdrawal(
                command.Amount,
                command.DestinationBank);
            
            if (!result.IsSuccess)
                return new WithdrawalResultDto { IsSuccess = false, Message = result.Message };
            
            // 트랜잭션 기록
            var transaction = Transaction.CreateWithdrawal(
                account.Id,
                command.Amount,
                command.DestinationBank);
            
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.UpdateAsync(account);
            await _unitOfWork.CommitAsync();
            
            // 브로커에 출금 요청
            await _kiwoomApi.RequestWithdrawalAsync(
                account.AccountNumber.Value,
                command.Amount,
                command.DestinationBank);
            
            // 이벤트 발행
            foreach (var @event in account.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return new WithdrawalResultDto
            {
                IsSuccess = true,
                WithdrawalId = result.WithdrawalId.ToString(),
                Amount = command.Amount,
                Message = "Withdrawal request processed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing withdrawal: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 포지션 크기 검증
    /// </summary>
    public async Task<PositionSizeValidationDto> ValidatePositionSizeAsync(
        AccountId accountId,
        Money positionValue,
        StockCode stockCode)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
            throw new AccountNotFoundException($"Account {accountId} not found");
        
        var validation = account.ValidatePositionSize(positionValue, stockCode);
        
        return new PositionSizeValidationDto
        {
            IsValid = validation.IsValid,
            Errors = validation.Errors.ToList(),
            Warnings = validation.Warnings.ToList()
        };
    }
    
    /// <summary>
    /// 성과 지표 재계산
    /// </summary>
    public async Task RecalculatePerformanceMetricsAsync(
        AccountId accountId,
        IEnumerable<Position> positions,
        IEnumerable<Trade> trades,
        MarketData currentPrices)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
            throw new AccountNotFoundException($"Account {accountId} not found");
        
        account.RecalculatePerformanceMetrics(positions, trades, currentPrices);
        
        await _accountRepository.UpdateAsync(account);
        await _unitOfWork.CommitAsync();
        
        // 이벤트 발행
        foreach (var @event in account.DomainEvents)
        {
            await _eventBus.PublishAsync(@event);
        }
    }
}

/// <summary>
/// 계좌 계산 서비스
/// 복잡한 재무 계산 담당
/// </summary>
public sealed class AccountCalculationService : IAccountCalculationService
{
    private readonly IMarketDataService _marketDataService;
    private readonly ITaxRateProvider _taxRateProvider;
    private readonly ILogger<AccountCalculationService> _logger;
    
    public AccountCalculationService(
        IMarketDataService marketDataService,
        ITaxRateProvider taxRateProvider,
        ILogger<AccountCalculationService> logger)
    {
        _marketDataService = marketDataService;
        _taxRateProvider = taxRateProvider;
        _logger = logger;
    }
    
    /// <summary>
    /// 매수 여력 계산
    /// </summary>
    public Money CalculateBuyingPower(TradingAccount account)
    {
        return account.Type switch
        {
            AccountType.Cash =>
                account.AvailableBalance,
            
            AccountType.Margin =>
                account.AvailableBalance * account.Margin.MarginMultiplier,
            
            AccountType.DayTrading =>
                account.AvailableBalance * 4m,
            
            AccountType.Futures =>
                account.AvailableBalance * 10m,
            
            _ => Money.Zero
        };
    }
    
    /// <summary>
    /// 성과 지표 계산
    /// </summary>
    public PerformanceMetrics CalculatePerformance(
        IEnumerable<Trade> trades,
        Money initialCapital)
    {
        var tradeList = trades.ToList();
        
        if (tradeList.Count == 0)
            return new PerformanceMetrics();
        
        var totalReturn = tradeList.Sum(t => t.ProfitLoss);
        var winningTrades = tradeList.Where(t => t.ProfitLoss > 0).ToList();
        var losingTrades = tradeList.Where(t => t.ProfitLoss < 0).ToList();
        
        var winRate = (decimal)winningTrades.Count / tradeList.Count;
        var profitFactor = CalculateProfitFactor(winningTrades, losingTrades);
        var sharpeRatio = CalculateSharpeRatio(tradeList);
        var maxDrawdown = CalculateMaxDrawdown(tradeList);
        
        return new PerformanceMetrics
        {
            TotalReturn = initialCapital.Amount > 0 ? (totalReturn / initialCapital) : 0,
            SharpeRatio = sharpeRatio,
            MaxDrawdown = maxDrawdown,
            WinRate = winRate,
            ProfitFactor = profitFactor,
            AverageWin = winningTrades.Any()
                ? new Money(winningTrades.Average(t => t.ProfitLoss.Amount))
                : Money.Zero,
            AverageLoss = losingTrades.Any()
                ? new Money(losingTrades.Average(t => t.ProfitLoss.Amount))
                : Money.Zero
        };
    }
    
    private decimal CalculateProfitFactor(
        List<Trade> winningTrades,
        List<Trade> losingTrades)
    {
        var totalWins = winningTrades.Sum(t => t.ProfitLoss.Amount);
        var totalLosses = Math.Abs(losingTrades.Sum(t => t.ProfitLoss.Amount));
        
        return totalLosses > 0 ? totalWins / totalLosses : decimal.MaxValue;
    }
    
    private decimal CalculateSharpeRatio(List<Trade> trades)
    {
        if (trades.Count < 2) return 0;
        
        var returns = trades.Select(t => t.ProfitLoss.Amount / 1000000m).ToList();
        var avgReturn = returns.Average();
        var variance = returns.Sum(r => (r - avgReturn) * (r - avgReturn)) / returns.Count;
        var stdDev = (decimal)Math.Sqrt((double)variance);
        
        return stdDev > 0 ? (avgReturn / stdDev) * (decimal)Math.Sqrt(252) : 0;
    }
    
    private Money CalculateMaxDrawdown(List<Trade> trades)
    {
        if (!trades.Any()) return Money.Zero;
        
        var peak = Money.Zero;
        var maxDD = Money.Zero;
        var cumulative = Money.Zero;
        
        foreach (var trade in trades.OrderBy(t => t.ClosedAt))
        {
            cumulative += trade.ProfitLoss;
            
            if (cumulative > peak)
                peak = cumulative;
            
            var drawdown = peak - cumulative;
            if (drawdown > maxDD)
                maxDD = drawdown;
        }
        
        return maxDD;
    }
}
```

---

## 6. Infrastructure Layer

```csharp
/// <summary>
/// 계좌 저장소 구현
/// </summary>
public sealed class AccountRepository : IAccountRepository
{
    private readonly AccountDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AccountRepository> _logger;
    
    public AccountRepository(
        AccountDbContext context,
        IDistributedCache cache,
        ILogger<AccountRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// ID로 계좌 조회
    /// </summary>
    public async Task<TradingAccount> GetByIdAsync(AccountId accountId)
    {
        var cacheKey = $"account:{accountId}";
        
        // 캐시 확인
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<TradingAccount>(cachedData);
        }
        
        // DB 조회
        var account = await _context.Accounts
            .AsNoTracking()
            .Include(a => a._balances)
            .Include(a => a._restrictions)
            .Include(a => a._performanceHistory)
            .FirstOrDefaultAsync(a => a.Id == accountId);
        
        if (account == null)
            return null;
        
        // 캐시 저장 (5분)
        var json = JsonSerializer.Serialize(account);
        await _cache.SetStringAsync(cacheKey, json, TimeSpan.FromMinutes(5));
        
        return account;
    }
    
    /// <summary>
    /// 계좌 저장
    /// </summary>
    public async Task AddAsync(TradingAccount account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        
        // 캐시 무효화
        await InvalidateCacheAsync(account.Id);
        
        _logger.LogInformation($"Account {account.Id} added");
    }
    
    /// <summary>
    /// 계좌 업데이트
    /// </summary>
    public async Task UpdateAsync(TradingAccount account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
        
        // 캐시 무효화
        await InvalidateCacheAsync(account.Id);
        
        _logger.LogInformation($"Account {account.Id} updated");
    }
    
    private async Task InvalidateCacheAsync(AccountId accountId)
    {
        var cacheKey = $"account:{accountId}";
        await _cache.RemoveAsync(cacheKey);
    }
}

/// <summary>
/// 키움 API 계정 어댑터
/// </summary>
public sealed class KiwoomAccountAdapter : IAccountDataProvider
{
    private readonly IKiwoomApi _kiwoomApi;
    private readonly ILogger<KiwoomAccountAdapter> _logger;
    
    public KiwoomAccountAdapter(
        IKiwoomApi kiwoomApi,
        ILogger<KiwoomAccountAdapter> logger)
    {
        _kiwoomApi = kiwoomApi;
        _logger = logger;
    }
    
    /// <summary>
    /// 브로커로부터 계좌 데이터 조회
    /// </summary>
    public async Task<AccountData> GetAccountDataAsync(string accountNumber)
    {
        try
        {
            var balance = await _kiwoomApi.GetBalanceAsync(accountNumber);
            var evaluation = await _kiwoomApi.GetEvaluationAsync(accountNumber);
            var margin = await _kiwoomApi.GetMarginInfoAsync(accountNumber);
            
            return new AccountData
            {
                CashBalance = new Money(balance.Cash),
                TotalAssets = new Money(balance.TotalAssets),
                BuyingPower = new Money(balance.BuyingPower),
                TotalEvaluation = new Money(evaluation.TotalValue),
                UnrealizedPL = new Money(evaluation.UnrealizedPL),
                RealizedPL = new Money(evaluation.RealizedPL),
                MarginBalance = new Money(margin.MarginBalance),
                MaintenanceMargin = new Money(margin.MaintenanceMargin),
                IsMarginCallActive = margin.IsMarginCallActive,
                SyncedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting account data from Kiwoom: {ex.Message}");
            throw;
        }
    }
}
```

---

## 7. API/Interface Specifications

```csharp
/// <summary>
/// 계좌 API
/// </summary>
[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    /// <summary>
    /// GET /api/accounts/{accountId}
    /// 계좌 정보 조회
    /// </summary>
    [HttpGet("{accountId}")]
    public async Task<ActionResult<AccountSummaryDto>> GetAccountAsync(string accountId)
    {
        var query = new GetAccountQuery(AccountId.Parse(accountId));
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    /// <summary>
    /// POST /api/accounts/{accountId}/sync
    /// 계좌 정보 동기화
    /// </summary>
    [HttpPost("{accountId}/sync")]
    public async Task<ActionResult> SyncAccountAsync(string accountId)
    {
        var command = new SyncAccountCommand(AccountId.Parse(accountId));
        await _mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// POST /api/accounts/{accountId}/withdrawals
    /// 출금 요청
    /// </summary>
    [HttpPost("{accountId}/withdrawals")]
    public async Task<ActionResult<WithdrawalResultDto>> RequestWithdrawalAsync(
        string accountId,
        [FromBody] WithdrawalRequestDto request)
    {
        var command = new RequestWithdrawalCommand(
            AccountId.Parse(accountId),
            request.Amount,
            request.BankCode,
            request.AccountNumber);
        
        var result = await _mediator.Send(command);
        return Created($"/api/accounts/{accountId}/withdrawals/{result.WithdrawalId}", result);
    }
    
    /// <summary>
    /// GET /api/accounts/{accountId}/performance
    /// 성과 지표 조회
    /// </summary>
    [HttpGet("{accountId}/performance")]
    public async Task<ActionResult<PerformanceMetricsDto>> GetPerformanceAsync(string accountId)
    {
        var query = new GetPerformanceQuery(AccountId.Parse(accountId));
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

/// <summary>
/// 계좌 저장소 인터페이스
/// </summary>
public interface IAccountRepository
{
    Task<TradingAccount> GetByIdAsync(AccountId accountId);
    Task<TradingAccount> GetByAccountNumberAsync(AccountNumber accountNumber);
    Task<IEnumerable<TradingAccount>> GetByUserIdAsync(UserId userId);
    Task AddAsync(TradingAccount account);
    Task UpdateAsync(TradingAccount account);
    Task DeleteAsync(AccountId accountId);
}

/// <summary>
/// 계좌 계산 서비스 인터페이스
/// </summary>
public interface IAccountCalculationService
{
    Money CalculateBuyingPower(TradingAccount account);
    PerformanceMetrics CalculatePerformance(IEnumerable<Trade> trades, Money initialCapital);
    Money CalculateCommission(Money executionValue, SecurityType securityType);
    Money CalculateTax(Money gain, TaxProfile taxProfile, int holdingDays);
    decimal CalculateTaxRate(int holdingDays, TaxProfile taxProfile);
}

/// <summary>
/// 계좌 데이터 제공자 인터페이스
/// </summary>
public interface IAccountDataProvider
{
    Task<AccountData> GetAccountDataAsync(string accountNumber);
}
```

---

## 8. Error Handling & Validation

```csharp
/// <summary>
/// 계좌 관련 사용자 정의 예외
/// </summary>

/// <summary>계좌를 찾을 수 없음</summary>
public sealed class AccountNotFoundException : DomainException
{
    public AccountId AccountId { get; }
    
    public AccountNotFoundException(string message, AccountId accountId = null)
        : base(message)
    {
        AccountId = accountId;
    }
}

/// <summary>잔고 부족</summary>
public sealed class InsufficientFundsException : DomainException
{
    public Money Required { get; }
    public Money Available { get; }
    
    public InsufficientFundsException(
        string message,
        Money required = null,
        Money available = null)
        : base(message)
    {
        Required = required;
        Available = available;
    }
}

/// <summary>출금 한도 초과</summary>
public sealed class WithdrawalLimitExceededException : DomainException
{
    public Money DailyLimit { get; }
    public Money Requested { get; }
    
    public WithdrawalLimitExceededException(
        string message,
        Money dailyLimit = null,
        Money requested = null)
        : base(message)
    {
        DailyLimit = dailyLimit;
        Requested = requested;
    }
}

/// <summary>계좌 제약 위반</summary>
public sealed class AccountRestrictionException : DomainException
{
    public RestrictionType RestrictionType { get; }
    
    public AccountRestrictionException(
        string message,
        RestrictionType restrictionType = null)
        : base(message)
    {
        RestrictionType = restrictionType;
    }
}

/// <summary>마진콜 발생</summary>
public sealed class MarginCallException : DomainException
{
    public Money RequiredMargin { get; }
    public Money CurrentMargin { get; }
    
    public MarginCallException(
        string message,
        Money requiredMargin,
        Money currentMargin)
        : base(message)
    {
        RequiredMargin = requiredMargin;
        CurrentMargin = currentMargin;
    }
}

/// <summary>
/// 포지션 크기 검증 결과
/// </summary>
public sealed class PositionSizeValidation
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();
    
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();
    public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
    
    public bool IsValid => _errors.Count == 0;
    
    public void AddError(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            _errors.Add(message);
    }
    
    public void AddWarning(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            _warnings.Add(message);
    }
}
```

---

## 9. External System Integration

```csharp
/// <summary>
/// 키움 API 서비스 인터페이스
/// </summary>
public interface IKiwoomApiService
{
    /// <summary>계좌 잔고 조회</summary>
    Task<AccountBalanceData> GetAccountBalanceAsync(string accountNumber);
    
    /// <summary>계좌 평가 정보 조회</summary>
    Task<AccountEvaluationData> GetAccountEvaluationAsync(string accountNumber);
    
    /// <summary>포지션 조회</summary>
    Task<PositionData[]> GetPositionsAsync(string accountNumber);
    
    /// <summary>출금 요청</summary>
    Task<WithdrawalResponseData> RequestWithdrawalAsync(
        string accountNumber,
        Money amount,
        BankAccount destination);
    
    /// <summary>증거금 정보 조회</summary>
    Task<MarginInfoData> GetMarginInfoAsync(string accountNumber);
}

/// <summary>
/// 세금 정보 제공자 인터페이스
/// </summary>
public interface ITaxRateProvider
{
    /// <summary>세율 조회</summary>
    decimal GetTaxRate(TaxResidentType residentType, int holdingDays);
    
    /// <summary>거래세 조회</summary>
    decimal GetTradingTaxRate(SecurityType securityType);
}

/// <summary>
/// 마진 정책 제공자 인터페이스
/// </summary>
public interface IMarginPolicyProvider
{
    /// <summary>종목별 마진 정책 조회</summary>
    MarginPolicy GetPolicy(StockCode stockCode);
}

/// <summary>
/// 수수료 정책 제공자 인터페이스
/// </summary>
public interface ICommissionPolicyProvider
{
    /// <summary>증권 유형별 수수료 정책 조회</summary>
    CommissionPolicy GetPolicy(SecurityType securityType);
}
```

---

## 10. Performance Considerations

### 10.1 캐싱 전략

```csharp
public sealed class AccountCachingService
{
    private readonly IDistributedCache _cache;
    private readonly IAccountRepository _repository;
    
    /// <summary>
    /// 캐시 전략
    /// - L1: Redis (1분 TTL) - 빠른 조회
    /// - L2: Database - 원본 데이터
    /// </summary>
    public async Task<TradingAccount> GetAccountWithCacheAsync(AccountId accountId)
    {
        const string cacheKeyPrefix = "account";
        const int cacheDurationMinutes = 5;
        
        var cacheKey = $"{cacheKeyPrefix}:{accountId}";
        
        // L1 캐시 (Redis) 확인
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<TradingAccount>(cachedData);
        }
        
        // L2 캐시 (DB) 조회
        var account = await _repository.GetByIdAsync(accountId);
        
        if (account != null)
        {
            // Redis에 저장
            var json = JsonSerializer.Serialize(account);
            await _cache.SetStringAsync(
                cacheKey,
                json,
                TimeSpan.FromMinutes(cacheDurationMinutes));
        }
        
        return account;
    }
    
    /// <summary>
    /// 계좌 캐시 무효화
    /// </summary>
    public async Task InvalidateAccountCacheAsync(AccountId accountId)
    {
        var cacheKey = $"account:{accountId}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

### 10.2 쿼리 최적화

```csharp
public sealed class OptimizedAccountRepository : IAccountRepository
{
    private readonly AccountDbContext _context;
    
    /// <summary>
    /// 쿼리 최적화된 계좌 조회
    /// - 필요한 컬럼만 선택
    /// - N+1 문제 해결 (Include 사용)
    /// - 인덱스 활용
    /// </summary>
    public async Task<TradingAccount> GetByIdOptimizedAsync(AccountId accountId)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.Id == accountId)
            .Include(a => a._balances)
            .Include(a => a._restrictions)
            .ThenInclude(r => r.RestrictionPolicy)
            .FirstOrDefaultAsync();
    }
}
```

### 10.3 배치 처리

```csharp
/// <summary>
/// 배치 성과 지표 재계산
/// 모든 활성 계좌의 성과 지표를 배치로 재계산
/// </summary>
public sealed class BatchPerformanceUpdateJob : IBackgroundJob
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountCalculationService _calculationService;
    
    public async Task ExecuteAsync()
    {
        const int batchSize = 100;
        var skip = 0;
        
        while (true)
        {
            var accounts = await _accountRepository
                .GetActiveAccountsAsync(skip, batchSize);
            
            if (!accounts.Any())
                break;
            
            foreach (var account in accounts)
            {
                // 성과 지표 재계산
                // ...
            }
            
            skip += batchSize;
        }
    }
}
```

---

## 11. Security Considerations

### 11.1 접근 제어

```csharp
/// <summary>
/// 계좌 접근 제어
/// </summary>
public sealed class AccountAccessControl
{
    private readonly IAuthorizationService _authorizationService;
    
    /// <summary>
    /// 사용자가 계좌에 접근할 수 있는지 확인
    /// </summary>
    public async Task<bool> CanAccessAccountAsync(
        UserId userId,
        AccountId accountId)
    {
        // 1. 계좌 소유자 확인
        var account = await GetAccountAsync(accountId);
        if (account.OwnerId != userId)
            return false;
        
        // 2. 관리자 권한 확인
        var isAdmin = await _authorizationService
            .IsInRoleAsync(userId, "Admin");
        
        return isAdmin || account.OwnerId == userId;
    }
}
```

### 11.2 감사 로깅

```csharp
/// <summary>
/// 계좌 감사 로깅
/// </summary>
public sealed class AccountAuditLogger
{
    private readonly ILogger<AccountAuditLogger> _auditLogger;
    
    /// <summary>
    /// 민감한 작업 로깅
    /// </summary>
    public void LogSensitiveOperation(
        string operation,
        AccountId accountId,
        UserId userId,
        object details)
    {
        _auditLogger.LogWarning(
            "AUDIT: {Operation} - Account: {AccountId}, User: {UserId}, Details: {Details}",
            operation,
            accountId,
            userId,
            details);
    }
}
```

### 11.3 데이터 암호화

```csharp
/// <summary>
/// 계좌 정보 암호화
/// </summary>
public sealed class AccountEncryption
{
    private readonly IEncryptionService _encryptionService;
    
    /// <summary>
    /// 민감한 필드 암호화
    /// </summary>
    public string EncryptBankAccount(BankAccount bankAccount)
    {
        var json = JsonSerializer.Serialize(bankAccount);
        return _encryptionService.Encrypt(json);
    }
    
    /// <summary>
    /// 민감한 필드 복호화
    /// </summary>
    public BankAccount DecryptBankAccount(string encryptedData)
    {
        var json = _encryptionService.Decrypt(encryptedData);
        return JsonSerializer.Deserialize<BankAccount>(json);
    }
}
```

---

## 결론

이 Account Context 설계는 다음 특징을 갖습니다:

✅ **완전성**: 계좌 관리의 모든 측면을 다룸
✅ **안전성**: 자금 안전성을 최우선으로 고려
✅ **성능**: 캐싱, 최적화, 배치 처리
✅ **확장성**: CQRS, Event Sourcing 적용 가능
✅ **규제 준수**: 세금, 감시, 감사 로깅
✅ **통합성**: 브로커 API와 완벽 통합

이 설계를 기반으로 견고하고 확장 가능한 거래 시스템을 구축할 수 있습니다.
