# Market Data Context - DDD 상세 설계

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

**Market Data Context**는 모든 시장 데이터의 수집, 처리, 저장, 제공을 담당하는 핵심 Bounded Context입니다.

#### 주요 책임
- ✅ 키움 OpenAPI를 통한 실시간 시세 수신
- ✅ 현재가, 호가, 거래량 등 시세 정보 관리
- ✅ 차트 데이터 생성 및 관리 (1분/5분/1시간/일봉 등)
- ✅ 기술적 지표 계산 (RSI, MACD, 볼린저 밴드 등)
- ✅ 차트 패턴 인식 (헤드앤숄더, 삼각수렴 등)
- ✅ 시장 데이터 이벤트 발행 (급등/급락/거래량 급증)
- ✅ 과거 데이터 조회 및 시계열 데이터 제공
- ✅ 데이터 품질 검증 및 이상치 탐지

#### 핵심 원칙
- **실시간 성능**: 밀리초 단위 응답 시간
- **데이터 정확성**: 모든 시세 데이터의 정확성 보증
- **시스템 안정성**: 데이터 손실 없는 신뢰성
- **확장성**: 수천 개 종목의 실시간 처리
- **인계속성**: 끊김 없는 실시간 데이터 수신

### 1.2 Context Map

```
┌──────────────────────────────────────────────────────────┐
│              Market Data Context                         │
│           (시장 데이터 수집 및 처리)                      │
└──────────────────────────────────────────────────────────┘
    ↑                                      ↓ 시세 정보
    │ 시세 수신                            │ 차트/지표
    │                                      │
┌───┴──────┐                          ┌───┴──────────────┐
│ Kiwoom   │                          │  Trading Context │
│  API     │                          │  (거래 실행)     │
└──────────┘                          └──────────────────┘
                                              ↑
                                              │ 리스크 검증
                                      ┌───────┴──────────┐
                                      │ Risk Management  │
                                      │  (리스크 관리)   │
                                      └──────────────────┘

Strategy Context ← 기술 지표 신호
  (전략)
```

### 1.3 바운더리

**이 Context가 담당하는 것:**
- 실시간 시세 데이터 수신 및 관리
- 차트 데이터 생성 및 저장
- 기술적 지표 계산
- 데이터 검증 및 정제
- 시장 데이터 이벤트 발행

**이 Context가 담당하지 않는 것:**
- 거래 실행 (Trading Context)
- 리스크 관리 (Risk Management Context)
- 포지션 관리 (Account Context)
- 전략 실행 (Strategy Context)

---

## 2. Domain Model 상세 설계

### 2.1 Aggregates

#### 2.1.1 MarketData Aggregate (실시간 시세)

**Aggregate Root**

```csharp
/// <summary>
/// 실시간 시세 데이터 Aggregate Root
/// 특정 종목의 현재 시세 정보를 종합적으로 관리
/// </summary>
public class MarketData : AggregateRoot<MarketDataId>
{
    private readonly List<PriceLevel> _orderBook = new();
    private readonly List<Execution> _recentExecutions = new();
    private readonly List<VolumeProfile> _volumeProfile = new();
    
    // ======================== Identity ========================
    /// <summary>시장 데이터 고유 ID</summary>
    public MarketDataId Id { get; private set; }
    
    /// <summary>종목 코드</summary>
    public StockCode StockCode { get; private set; }
    
    // ======================== Price Information ========================
    /// <summary>현재가</summary>
    public MarketPrice CurrentPrice { get; private set; }
    
    /// <summary>시가 (오늘)</summary>
    public MarketPrice OpenPrice { get; private set; }
    
    /// <summary>고가 (오늘)</summary>
    public MarketPrice HighPrice { get; private set; }
    
    /// <summary>저가 (오늘)</summary>
    public MarketPrice LowPrice { get; private set; }
    
    /// <summary>전일 종가</summary>
    public MarketPrice PreviousClosePrice { get; private set; }
    
    /// <summary>현재 변동 정보</summary>
    public PriceChange Change { get; private set; }
    
    // ======================== Volume Information ========================
    /// <summary>거래량 (수량)</summary>
    public TradingVolume Volume { get; private set; }
    
    /// <summary>거래대금 (금액)</summary>
    public Money Amount { get; private set; }
    
    /// <summary>거래량 가중 평균가</summary>
    public MarketPrice VWAP { get; private set; }
    
    // ======================== Order Book ========================
    /// <summary>호가 (매수/매도 대기 주문)</summary>
    public IReadOnlyList<PriceLevel> OrderBook => _orderBook.AsReadOnly();
    
    /// <summary>최우선 매수호가</summary>
    public MarketPrice BidPrice1 { get; private set; }
    
    /// <summary>최우선 매도호가</summary>
    public MarketPrice AskPrice1 { get; private set; }
    
    /// <summary>호가 스프레드 (매도호가 - 매수호가)</summary>
    public decimal Spread { get; private set; }
    
    // ======================== Recent Executions ========================
    /// <summary>최근 체결 기록</summary>
    public IReadOnlyList<Execution> RecentExecutions => _recentExecutions.AsReadOnly();
    
    /// <summary>최근 대량 체결 여부</summary>
    public bool HasLargeExecution { get; private set; }
    
    // ======================== Market Status ========================
    /// <summary>장 상태 (거래 전/개시/정규장/종가/마감/정지)</summary>
    public MarketStatus Status { get; private set; }
    
    /// <summary>거래 가능 여부</summary>
    public bool IsTradeable { get; private set; }
    
    /// <summary>상한가 여부</summary>
    public bool IsUpperLimit { get; private set; }
    
    /// <summary>하한가 여부</summary>
    public bool IsLowerLimit { get; private set; }
    
    // ======================== Statistical Metrics ========================
    /// <summary>누적 매수량</summary>
    public TradingVolume AccumulatedBidVolume { get; private set; }
    
    /// <summary>누적 매도량</summary>
    public TradingVolume AccumulatedAskVolume { get; private set; }
    
    /// <summary>거래량 비중 (매수/매도 비율)</summary>
    public decimal BidAskRatio { get; private set; }
    
    // ======================== Metadata ========================
    /// <summary>데이터 수신 시간</summary>
    public DateTime ReceivedAt { get; private set; }
    
    /// <summary>마지막 업데이트 시간</summary>
    public DateTime LastUpdatedAt { get; private set; }
    
    /// <summary>데이터 신뢰도 (0~1)</summary>
    public decimal DataReliability { get; private set; }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 현재가 업데이트
    /// </summary>
    public void UpdateCurrentPrice(
        MarketPrice newPrice,
        TradingVolume volume,
        Money amount,
        DateTime updateTime)
    {
        // Invariant: 가격 유효성 검증
        if (newPrice == null)
            throw new InvalidMarketDataException("Price cannot be null");
        
        // Invariant: 상한가/하한가 범위 확인
        if (!ValidatePriceWithinLimits(newPrice))
            throw new InvalidMarketDataException(
                $"Price {newPrice} exceeds price limits");
        
        var previousPrice = CurrentPrice;
        var previousVolume = Volume;
        
        // 현재가 업데이트
        CurrentPrice = newPrice;
        Volume = Volume.Add(volume);
        Amount = Amount.Add(amount);
        
        // 고가/저가 업데이트
        if (OpenPrice == null)
            OpenPrice = newPrice;
        
        if (HighPrice == null || newPrice > HighPrice)
            HighPrice = newPrice;
        
        if (LowPrice == null || newPrice < LowPrice)
            LowPrice = newPrice;
        
        // 변동 정보 계산
        if (PreviousClosePrice != null)
        {
            Change = PriceChange.Calculate(PreviousClosePrice, newPrice);
        }
        
        // VWAP 업데이트
        if (Volume.Value > 0)
        {
            VWAP = new MarketPrice(Amount.Amount / Volume.Value);
        }
        
        // 메타데이터 업데이트
        LastUpdatedAt = updateTime;
        DataReliability = Math.Min(1.0m, DataReliability + 0.05m);
        
        // 도메인 이벤트 발행
        AddDomainEvent(new PriceUpdatedEvent(
            Id,
            StockCode,
            newPrice,
            previousPrice,
            volume,
            updateTime));
        
        // 급등/급락 감지
        if (Change != null && Math.Abs(Change.ChangeRate) >= 5.0m)
        {
            var alertType = Change.ChangeRate >= 5.0m
                ? AlertType.SurgeAlert
                : AlertType.PlungeAlert;
            
            AddDomainEvent(new PriceExtremeEvent(
                Id,
                StockCode,
                newPrice,
                Change,
                alertType,
                updateTime));
        }
        
        // 거래량 급증 감지
        if (previousVolume != null && volume.Value > 0)
        {
            var volumeIncreaseRate = (volume.Value / (decimal)previousVolume.Value) * 100;
            if (volumeIncreaseRate >= 300) // 300% 이상 증가
            {
                AddDomainEvent(new VolumeAnomalyEvent(
                    Id,
                    StockCode,
                    volume,
                    volumeIncreaseRate,
                    updateTime));
            }
        }
    }
    
    /// <summary>
    /// 호가 업데이트 (호가창 정보)
    /// </summary>
    public void UpdateOrderBook(
        List<PriceLevel> bidLevels,
        List<PriceLevel> askLevels,
        DateTime updateTime)
    {
        if (bidLevels == null || askLevels == null)
            throw new InvalidMarketDataException("Order book data cannot be null");
        
        _orderBook.Clear();
        _orderBook.AddRange(bidLevels);
        _orderBook.AddRange(askLevels);
        
        // 최우선 호가 업데이트
        if (bidLevels.Any())
        {
            var topBid = bidLevels.First();
            BidPrice1 = topBid.Price;
        }
        
        if (askLevels.Any())
        {
            var topAsk = askLevels.First();
            AskPrice1 = topAsk.Price;
        }
        
        // 호가 스프레드 계산
        if (BidPrice1 != null && AskPrice1 != null)
        {
            Spread = (AskPrice1.Value - BidPrice1.Value) / BidPrice1.Value;
        }
        
        // 호가 불균형 감지
        var totalBidVolume = bidLevels.Sum(b => b.Volume.Value);
        var totalAskVolume = askLevels.Sum(a => a.Volume.Value);
        var total = totalBidVolume + totalAskVolume;
        
        if (total > 0)
        {
            BidAskRatio = (decimal)totalBidVolume / total;
            
            // 심각한 불균형 감지 (80% 이상 한쪽 쏠림)
            if (BidAskRatio >= 0.8m || BidAskRatio <= 0.2m)
            {
                var imbalanceType = BidAskRatio >= 0.8m
                    ? ImbalanceType.BidHeavy
                    : ImbalanceType.AskHeavy;
                
                AddDomainEvent(new OrderBookImbalanceEvent(
                    Id,
                    StockCode,
                    BidAskRatio,
                    imbalanceType,
                    updateTime));
            }
        }
        
        LastUpdatedAt = updateTime;
    }
    
    /// <summary>
    /// 체결 데이터 추가
    /// </summary>
    public void AddExecution(
        Execution execution)
    {
        if (execution == null)
            throw new ArgumentNullException(nameof(execution));
        
        _recentExecutions.Add(execution);
        
        // 최근 100건만 유지 (메모리 효율성)
        if (_recentExecutions.Count > 100)
        {
            _recentExecutions.RemoveAt(0);
        }
        
        // 대량 체결 감지
        if (execution.Volume.Value > 10000) // 10000주 이상
        {
            HasLargeExecution = true;
            
            AddDomainEvent(new LargeExecutionEvent(
                Id,
                StockCode,
                execution.Price,
                execution.Volume,
                execution.ExecutionTime));
        }
        
        LastUpdatedAt = execution.ExecutionTime;
    }
    
    /// <summary>
    /// 장 상태 변경
    /// </summary>
    public void UpdateMarketStatus(
        MarketStatus newStatus,
        DateTime updateTime)
    {
        if (Status == newStatus)
            return;
        
        var previousStatus = Status;
        Status = newStatus;
        
        // 장 상태에 따른 거래 가능성 업데이트
        IsTradeable = newStatus == MarketStatus.Trading ||
                     newStatus == MarketStatus.Closing;
        
        AddDomainEvent(new MarketStatusChangedEvent(
            Id,
            StockCode,
            previousStatus,
            newStatus,
            updateTime));
        
        LastUpdatedAt = updateTime;
    }
    
    /// <summary>
    /// 상한가/하한가 설정
    /// </summary>
    public void UpdatePriceLimits(
        MarketPrice upperLimit,
        MarketPrice lowerLimit,
        DateTime updateTime)
    {
        var wasUpperLimit = IsUpperLimit;
        var wasLowerLimit = IsLowerLimit;
        
        IsUpperLimit = Math.Abs((CurrentPrice.Value - upperLimit.Value) / upperLimit.Value) < 0.001m;
        IsLowerLimit = Math.Abs((CurrentPrice.Value - lowerLimit.Value) / lowerLimit.Value) < 0.001m;
        
        // 상한가/하한가 진입/이탈 감지
        if (!wasUpperLimit && IsUpperLimit)
        {
            AddDomainEvent(new PriceLimitEvent(
                Id,
                StockCode,
                CurrentPrice,
                PriceLimitType.UpperLimit,
                updateTime));
        }
        else if (!wasLowerLimit && IsLowerLimit)
        {
            AddDomainEvent(new PriceLimitEvent(
                Id,
                StockCode,
                CurrentPrice,
                PriceLimitType.LowerLimit,
                updateTime));
        }
        
        LastUpdatedAt = updateTime;
    }
    
    // ======================== Private Helper Methods ========================
    
    private bool ValidatePriceWithinLimits(MarketPrice price)
    {
        // 상한가/하한가 범위 확인 (이전 종가의 30% 범위 내)
        if (PreviousClosePrice == null)
            return true; // 첫 데이터는 검증하지 않음
        
        var upperLimit = PreviousClosePrice.Value * 1.3m;
        var lowerLimit = PreviousClosePrice.Value * 0.7m;
        
        return price.Value >= lowerLimit && price.Value <= upperLimit;
    }
    
    // Factory Methods
    public static MarketData Create(StockCode stockCode, MarketPrice previousClose)
    {
        return new MarketData
        {
            Id = MarketDataId.New(),
            StockCode = stockCode,
            PreviousClosePrice = previousClose,
            Status = MarketStatus.PreOpen,
            IsTradeable = false,
            ReceivedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            DataReliability = 0
        };
    }
}
```

#### 2.1.2 ChartData Aggregate (차트 데이터)

```csharp
/// <summary>
/// 차트 데이터 Aggregate Root
/// 특정 종목의 시간 주기별 OHLCV 데이터 관리
/// </summary>
public class ChartData : AggregateRoot<ChartDataId>
{
    private readonly List<Candle> _candles = new();
    private readonly List<ChartPattern> _detectedPatterns = new();
    
    // ======================== Identity ========================
    public ChartDataId Id { get; private set; }
    public StockCode StockCode { get; private set; }
    public ChartInterval Interval { get; private set; }
    
    // ======================== Candle Data ========================
    /// <summary>캔들 데이터 목록</summary>
    public IReadOnlyList<Candle> Candles => _candles.AsReadOnly();
    
    /// <summary>데이터 범위: 시작</summary>
    public DateTime StartDate { get; private set; }
    
    /// <summary>데이터 범위: 종료</summary>
    public DateTime EndDate { get; private set; }
    
    /// <summary>캔들 개수</summary>
    public int CandleCount => _candles.Count;
    
    // ======================== Pattern Analysis ========================
    /// <summary>감지된 차트 패턴</summary>
    public IReadOnlyList<ChartPattern> DetectedPatterns => _detectedPatterns.AsReadOnly();
    
    /// <summary>현재 형성 중인 패턴</summary>
    public List<PatternCandidate> PatternsForming { get; private set; } = new();
    
    // ======================== Statistical Data ========================
    /// <summary>기간 내 평균가</summary>
    public MarketPrice AveragePrice { get; private set; }
    
    /// <summary>기간 내 변동성 (표준편차)</summary>
    public decimal Volatility { get; private set; }
    
    /// <summary>총 거래량</summary>
    public TradingVolume TotalVolume { get; private set; }
    
    /// <summary>총 거래대금</summary>
    public Money TotalAmount { get; private set; }
    
    /// <summary>평균 거래량</summary>
    public decimal AverageVolume { get; private set; }
    
    // ======================== Trend Analysis ========================
    /// <summary>현재 추세 (상승/하락/횡보)</summary>
    public Trend CurrentTrend { get; private set; }
    
    /// <summary>지지선</summary>
    public List<SupportLevel> SupportLevels { get; private set; } = new();
    
    /// <summary>저항선</summary>
    public List<ResistanceLevel> ResistanceLevels { get; private set; } = new();
    
    // ======================== Metadata ========================
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 캔들 추가
    /// </summary>
    public void AddCandle(Candle candle)
    {
        if (candle == null)
            throw new ArgumentNullException(nameof(candle));
        
        // 중복 체크
        if (_candles.Any(c => c.Timestamp == candle.Timestamp))
            throw new DuplicateCandleException(
                $"Candle already exists for {candle.Timestamp}");
        
        // 캔들 추가
        _candles.Add(candle);
        _candles.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        
        // 메타데이터 업데이트
        if (_candles.Count == 1)
            StartDate = candle.Timestamp;
        EndDate = candle.Timestamp;
        
        // 통계 업데이트
        RecalculateStatistics();
        
        // 패턴 감지
        if (_candles.Count >= 3)
        {
            DetectPatterns();
            AnalyzeTrend();
            IdentifySupportResistance();
        }
        
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CandleAddedEvent(
            Id,
            StockCode,
            Interval,
            candle,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// 현재 캔들 업데이트 (진행 중인 캔들)
    /// </summary>
    public void UpdateCurrentCandle(
        MarketPrice price,
        TradingVolume volume,
        DateTime updateTime)
    {
        if (price == null || volume == null)
            throw new ArgumentNullException("Price and Volume cannot be null");
        
        // 현재 캔들인지 확인
        var currentCandle = _candles.LastOrDefault();
        
        if (currentCandle == null || !IsCurrentCandle(currentCandle, updateTime))
        {
            // 새 캔들 생성
            var newCandle = Candle.Create(
                GetCandleTimestamp(updateTime),
                price,
                price,
                price,
                price,
                volume);
            
            AddCandle(newCandle);
        }
        else
        {
            // 기존 캔들 업데이트
            currentCandle.Update(price, volume);
            LastUpdatedAt = updateTime;
        }
    }
    
    /// <summary>
    /// 차트 패턴 인식
    /// </summary>
    private void DetectPatterns()
    {
        if (_candles.Count < 5)
            return;
        
        // 주요 차트 패턴 감지
        DetectHeadAndShoulders();
        DetectTrianglePattern();
        DetectDoubleTop();
        DetectDoubleBottom();
        DetectCupAndHandle();
        DetectFlagPattern();
        DetectWedgePattern();
    }
    
    /// <summary>
    /// 헤드앤숄더 패턴 감지
    /// </summary>
    private void DetectHeadAndShoulders()
    {
        // 최소 5개 캔들 필요
        if (_candles.Count < 5) return;
        
        var recent = _candles.TakeLast(5).ToList();
        
        // 좌측 어깨 -> 머리 -> 우측 어깨 패턴
        var leftShoulder = recent[0];
        var head = recent[2];
        var rightShoulder = recent[4];
        
        // 머리가 양 어깨보다 높은지 확인
        if (head.High > leftShoulder.High && head.High > rightShoulder.High &&
            leftShoulder.High > rightShoulder.High)
        {
            var pattern = new ChartPattern(
                PatternType.HeadAndShoulders,
                PatternStrength.Medium,
                DateTime.UtcNow,
                "Bearish Head and Shoulders pattern detected");
            
            _detectedPatterns.Add(pattern);
            
            AddDomainEvent(new PatternDetectedEvent(
                Id,
                StockCode,
                Interval,
                pattern,
                DateTime.UtcNow));
        }
    }
    
    /// <summary>
    /// 삼각수렴 패턴 감지
    /// </summary>
    private void DetectTrianglePattern()
    {
        if (_candles.Count < 8) return;
        
        var recent = _candles.TakeLast(8).ToList();
        
        // 고가의 하강선과 저가의 상승선이 수렴하는지 확인
        var highs = recent.Select(c => c.High.Value).ToList();
        var lows = recent.Select(c => c.Low.Value).ToList();
        
        var highTrend = CalculateLinearTrend(highs);
        var lowTrend = CalculateLinearTrend(lows);
        
        // 둘 다 수렴하고 있는지 확인
        if (highTrend < 0 && lowTrend > 0)
        {
            var pattern = new ChartPattern(
                PatternType.TriangleConvergence,
                PatternStrength.High,
                DateTime.UtcNow,
                "Triangle pattern converging");
            
            PatternsForming.Add(new PatternCandidate(pattern, 0.8m));
        }
    }
    
    /// <summary>
    /// 추세 분석
    /// </summary>
    private void AnalyzeTrend()
    {
        if (_candles.Count < 3) return;
        
        var closes = _candles.Select(c => c.Close.Value).ToList();
        var trend = CalculateLinearTrend(closes);
        
        if (trend > 0.001m)
            CurrentTrend = Trend.Uptrend;
        else if (trend < -0.001m)
            CurrentTrend = Trend.Downtrend;
        else
            CurrentTrend = Trend.Sideways;
    }
    
    /// <summary>
    /// 지지선/저항선 식별
    /// </summary>
    private void IdentifySupportResistance()
    {
        if (_candles.Count < 10) return;
        
        var recent = _candles.TakeLast(10).ToList();
        
        // 지지선: 여러 번 터치된 저가
        var lows = recent.Select(c => c.Low.Value).OrderBy(x => x).ToList();
        if (lows.Count > 0)
        {
            var support = new SupportLevel(
                new MarketPrice(lows.First()),
                recent.Count(c => Math.Abs(c.Low.Value - lows.First()) < 1),
                DateTime.UtcNow);
            SupportLevels.Add(support);
        }
        
        // 저항선: 여러 번 터치된 고가
        var highs = recent.Select(c => c.High.Value).OrderByDescending(x => x).ToList();
        if (highs.Count > 0)
        {
            var resistance = new ResistanceLevel(
                new MarketPrice(highs.First()),
                recent.Count(c => Math.Abs(c.High.Value - highs.First()) < 1),
                DateTime.UtcNow);
            ResistanceLevels.Add(resistance);
        }
    }
    
    // ======================== Private Helper Methods ========================
    
    private void RecalculateStatistics()
    {
        if (_candles.Count == 0) return;
        
        // 평균가 계산
        var avgPrice = _candles.Average(c => c.Close.Value);
        AveragePrice = new MarketPrice(avgPrice);
        
        // 변동성 (표준편차) 계산
        var closes = _candles.Select(c => c.Close.Value).ToList();
        var variance = closes.Sum(c => Math.Pow((double)(c - (decimal)avgPrice), 2)) / closes.Count;
        Volatility = (decimal)Math.Sqrt(variance);
        
        // 거래량 통계
        TotalVolume = new TradingVolume(_candles.Sum(c => c.Volume.Value));
        AverageVolume = (decimal)TotalVolume.Value / _candles.Count;
    }
    
    private bool IsCurrentCandle(Candle candle, DateTime currentTime)
    {
        return Interval.IsInSameInterval(candle.Timestamp, currentTime);
    }
    
    private DateTime GetCandleTimestamp(DateTime dateTime)
    {
        return Interval.GetCandleStartTime(dateTime);
    }
    
    private decimal CalculateLinearTrend(List<decimal> values)
    {
        if (values.Count < 2) return 0;
        
        var n = values.Count;
        var sumX = Enumerable.Range(0, n).Sum(x => (decimal)x);
        var sumY = values.Sum();
        var sumXY = Enumerable.Range(0, n).Sum(i => (decimal)i * values[i]);
        var sumX2 = Enumerable.Range(0, n).Sum(x => (decimal)x * x);
        
        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }
    
    // Factory Method
    public static ChartData Create(StockCode stockCode, ChartInterval interval)
    {
        return new ChartData
        {
            Id = ChartDataId.New(),
            StockCode = stockCode,
            Interval = interval,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            CurrentTrend = Trend.Sideways
        };
    }
}
```

#### 2.1.3 TechnicalIndicator Aggregate (기술적 지표)

```csharp
/// <summary>
/// 기술적 지표 Aggregate Root
/// 특정 차트에 대한 기술적 지표 계산 및 신호 관리
/// </summary>
public class TechnicalIndicator : AggregateRoot<TechnicalIndicatorId>
{
    private readonly Dictionary<string, IndicatorValue> _values = new();
    private readonly List<IndicatorSignal> _signals = new();
    
    // ======================== Identity ========================
    public TechnicalIndicatorId Id { get; private set; }
    public StockCode StockCode { get; private set; }
    public IndicatorType Type { get; private set; }
    public ChartInterval Interval { get; private set; }
    
    // ======================== Configuration ========================
    /// <summary>지표 파라미터 (RSI period, MACD settings 등)</summary>
    public IndicatorParameters Parameters { get; private set; }
    
    // ======================== Values ========================
    /// <summary>계산된 지표 값들</summary>
    public IReadOnlyDictionary<string, IndicatorValue> Values => _values;
    
    /// <summary>지표 신호들</summary>
    public IReadOnlyList<IndicatorSignal> Signals => _signals.AsReadOnly();
    
    /// <summary>현재 신호 상태</summary>
    public SignalType CurrentSignal { get; private set; }
    
    // ======================== Metadata ========================
    public DateTime CalculatedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public decimal Confidence { get; private set; } // 신뢰도 (0~1)
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// RSI 지표 계산
    /// </summary>
    public void CalculateRSI(List<Candle> candles, int period = 14)
    {
        if (candles.Count < period)
            throw new InsufficientDataException($"Need at least {period} candles");
        
        var gains = 0m;
        var losses = 0m;
        
        // 초기 gain/loss 계산
        for (int i = 1; i < period; i++)
        {
            var change = candles[i].Close.Value - candles[i - 1].Close.Value;
            if (change > 0)
                gains += change;
            else
                losses += Math.Abs(change);
        }
        
        var avgGain = gains / period;
        var avgLoss = losses / period;
        
        // RSI 계산
        var rsiValues = new List<decimal>();
        for (int i = period; i < candles.Count; i++)
        {
            var change = candles[i].Close.Value - candles[i - 1].Close.Value;
            if (change > 0)
            {
                avgGain = (avgGain * (period - 1) + change) / period;
                avgLoss = (avgLoss * (period - 1)) / period;
            }
            else
            {
                avgGain = (avgGain * (period - 1)) / period;
                avgLoss = (avgLoss * (period - 1) + Math.Abs(change)) / period;
            }
            
            var rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            var rsi = 100 - (100 / (1 + rs));
            rsiValues.Add(rsi);
        }
        
        // 저장
        _values["RSI"] = new IndicatorValue(rsiValues.Last(), DateTime.UtcNow);
        
        // 신호 생성
        var rsi = rsiValues.Last();
        if (rsi > 70)
        {
            GenerateSignal(SignalType.Overbought, rsi, "RSI > 70");
        }
        else if (rsi < 30)
        {
            GenerateSignal(SignalType.Oversold, rsi, "RSI < 30");
        }
        
        CurrentSignal = rsi > 70 ? SignalType.Overbought :
                       rsi < 30 ? SignalType.Oversold :
                       SignalType.Neutral;
        
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// MACD 지표 계산
    /// </summary>
    public void CalculateMACD(List<Candle> candles, int fast = 12, int slow = 26, int signal = 9)
    {
        if (candles.Count < slow)
            throw new InsufficientDataException($"Need at least {slow} candles");
        
        var closes = candles.Select(c => c.Close.Value).ToList();
        
        // EMA 계산
        var ema12 = CalculateEMA(closes, fast);
        var ema26 = CalculateEMA(closes, slow);
        
        // MACD 라인
        var macdLine = ema12.Last() - ema26.Last();
        _values["MACD"] = new IndicatorValue(macdLine, DateTime.UtcNow);
        
        // 신호선 (MACD의 EMA)
        var macdValues = new List<decimal>();
        for (int i = Math.Max(fast, slow) - 1; i < closes.Count; i++)
        {
            var idx12 = i - (fast - 1);
            var idx26 = i - (slow - 1);
            macdValues.Add(ema12[idx12] - ema26[idx26]);
        }
        
        var signalLine = CalculateEMA(macdValues, signal).Last();
        _values["Signal"] = new IndicatorValue(signalLine, DateTime.UtcNow);
        
        // 히스토그램
        var histogram = macdLine - signalLine;
        _values["Histogram"] = new IndicatorValue(histogram, DateTime.UtcNow);
        
        // 신호 생성
        if (macdLine > signalLine && macdValues.Count >= 2 && 
            macdValues[macdValues.Count - 2] < signalLine)
        {
            GenerateSignal(SignalType.BullishCross, histogram, "MACD crosses above signal");
        }
        else if (macdLine < signalLine && macdValues.Count >= 2 && 
                 macdValues[macdValues.Count - 2] > signalLine)
        {
            GenerateSignal(SignalType.BearishCross, histogram, "MACD crosses below signal");
        }
        
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 볼린저 밴드 계산
    /// </summary>
    public void CalculateBollingerBands(List<Candle> candles, int period = 20, decimal stdDev = 2)
    {
        if (candles.Count < period)
            throw new InsufficientDataException($"Need at least {period} candles");
        
        var closes = candles.Select(c => c.Close.Value).ToList();
        var lastClose = closes.Last();
        
        // 이동평균
        var ma = closes.TakeLast(period).Average();
        _values["MiddleBand"] = new IndicatorValue(ma, DateTime.UtcNow);
        
        // 표준편차
        var variance = closes.TakeLast(period).Sum(c => Math.Pow((double)(c - (decimal)ma), 2)) / period;
        var std = (decimal)Math.Sqrt(variance);
        
        // 상단/하단 밴드
        var upperBand = ma + (std * stdDev);
        var lowerBand = ma - (std * stdDev);
        
        _values["UpperBand"] = new IndicatorValue(upperBand, DateTime.UtcNow);
        _values["LowerBand"] = new IndicatorValue(lowerBand, DateTime.UtcNow);
        
        // 신호 생성
        if (lastClose >= upperBand)
        {
            GenerateSignal(SignalType.UpperBandTouch, lastClose, "Price touches upper band");
        }
        else if (lastClose <= lowerBand)
        {
            GenerateSignal(SignalType.LowerBandTouch, lastClose, "Price touches lower band");
        }
        
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 신호 생성
    /// </summary>
    private void GenerateSignal(SignalType signalType, decimal value, string description)
    {
        var signal = new IndicatorSignal(
            signalType,
            value,
            description,
            DateTime.UtcNow);
        
        _signals.Add(signal);
        CurrentSignal = signalType;
        Confidence = Math.Min(1.0m, Confidence + 0.1m);
        
        AddDomainEvent(new IndicatorSignalGeneratedEvent(
            Id,
            StockCode,
            Type,
            Interval,
            signalType,
            value,
            DateTime.UtcNow));
    }
    
    // ======================== Private Helper Methods ========================
    
    private List<decimal> CalculateEMA(List<decimal> values, int period)
    {
        var emaValues = new List<decimal>();
        var multiplier = 2.0m / (period + 1);
        var sma = values.Take(period).Average();
        emaValues.Add(sma);
        
        for (int i = period; i < values.Count; i++)
        {
            var ema = (values[i] - emaValues.Last()) * multiplier + emaValues.Last();
            emaValues.Add(ema);
        }
        
        return emaValues;
    }
    
    // Factory Method
    public static TechnicalIndicator Create(
        StockCode stockCode,
        IndicatorType type,
        ChartInterval interval,
        IndicatorParameters parameters)
    {
        return new TechnicalIndicator
        {
            Id = TechnicalIndicatorId.New(),
            StockCode = stockCode,
            Type = type,
            Interval = interval,
            Parameters = parameters,
            CalculatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            Confidence = 0
        };
    }
}
```

### 2.2 Value Objects

```csharp
/// <summary>시장 데이터 ID</summary>
public sealed class MarketDataId : StronglyTypedId<Guid>
{
    public MarketDataId(Guid value) : base(value) { }
    public static MarketDataId New() => new(Guid.NewGuid());
}

/// <summary>차트 데이터 ID</summary>
public sealed class ChartDataId : StronglyTypedId<Guid>
{
    public ChartDataId(Guid value) : base(value) { }
    public static ChartDataId New() => new(Guid.NewGuid());
}

/// <summary>기술적 지표 ID</summary>
public sealed class TechnicalIndicatorId : StronglyTypedId<Guid>
{
    public TechnicalIndicatorId(Guid value) : base(value) { }
    public static TechnicalIndicatorId New() => new(Guid.NewGuid());
}

/// <summary>종목 코드</summary>
public sealed class StockCode : ValueObject
{
    public string Value { get; }
    
    public StockCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 6)
            throw new ArgumentException("Stock code must be 6 digits");
        if (!value.All(c => char.IsDigit(c)))
            throw new ArgumentException("Stock code must contain only digits");
        
        Value = value;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

/// <summary>시장 가격</summary>
public sealed class MarketPrice : ValueObject, IComparable<MarketPrice>
{
    public decimal Value { get; }
    
    public MarketPrice(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Price cannot be negative");
        
        Value = value;
    }
    
    public static bool operator >(MarketPrice left, MarketPrice right) => left.CompareTo(right) > 0;
    public static bool operator <(MarketPrice left, MarketPrice right) => left.CompareTo(right) < 0;
    public static bool operator >=(MarketPrice left, MarketPrice right) => left.CompareTo(right) >= 0;
    public static bool operator <=(MarketPrice left, MarketPrice right) => left.CompareTo(right) <= 0;
    
    public int CompareTo(MarketPrice other) => Value.CompareTo(other?.Value ?? 0);
    
    public override string ToString() => $"₩{Value:N0}";
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

/// <summary>거래량</summary>
public sealed class TradingVolume : ValueObject, IComparable<TradingVolume>
{
    public long Value { get; }
    
    public TradingVolume(long value)
    {
        if (value < 0)
            throw new ArgumentException("Volume cannot be negative");
        
        Value = value;
    }
    
    public TradingVolume Add(TradingVolume other)
    {
        if (other == null) return this;
        return new TradingVolume(Value + other.Value);
    }
    
    public int CompareTo(TradingVolume other) => Value.CompareTo(other?.Value ?? 0);
    
    public override string ToString() => $"{Value:N0}";
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

/// <summary>가격 변동 정보</summary>
public sealed class PriceChange : ValueObject
{
    public decimal ChangeAmount { get; }
    public decimal ChangeRate { get; } // 퍼센트
    public ChangeDirection Direction { get; }
    
    private PriceChange(decimal changeAmount, decimal changeRate)
    {
        ChangeAmount = changeAmount;
        ChangeRate = changeRate;
        Direction = changeRate > 0 ? ChangeDirection.Up :
                   changeRate < 0 ? ChangeDirection.Down :
                   ChangeDirection.Flat;
    }
    
    public static PriceChange Calculate(MarketPrice previousPrice, MarketPrice currentPrice)
    {
        if (previousPrice == null || previousPrice.Value == 0)
            return new PriceChange(0, 0);
        
        var changeAmount = currentPrice.Value - previousPrice.Value;
        var changeRate = (changeAmount / previousPrice.Value) * 100;
        
        return new PriceChange(changeAmount, changeRate);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ChangeAmount;
        yield return ChangeRate;
        yield return Direction;
    }
}

/// <summary>호가 수준</summary>
public sealed class PriceLevel : ValueObject
{
    public MarketPrice Price { get; }
    public TradingVolume Volume { get; }
    public int OrderCount { get; }
    public OrderSide Side { get; }
    
    public PriceLevel(MarketPrice price, TradingVolume volume, int orderCount, OrderSide side)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Volume = volume ?? throw new ArgumentNullException(nameof(volume));
        OrderCount = orderCount;
        Side = side;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Price;
        yield return Side;
    }
}

/// <summary>캔들 데이터</summary>
public sealed class Candle : ValueObject
{
    public DateTime Timestamp { get; private set; }
    public MarketPrice Open { get; private set; }
    public MarketPrice High { get; private set; }
    public MarketPrice Low { get; private set; }
    public MarketPrice Close { get; private set; }
    public TradingVolume Volume { get; private set; }
    public Money Amount { get; private set; }
    
    private Candle(
        DateTime timestamp,
        MarketPrice open,
        MarketPrice high,
        MarketPrice low,
        MarketPrice close,
        TradingVolume volume,
        Money amount = null)
    {
        Timestamp = timestamp;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        Amount = amount ?? new Money(0);
    }
    
    public static Candle Create(
        DateTime timestamp,
        MarketPrice open,
        MarketPrice high,
        MarketPrice low,
        MarketPrice close,
        TradingVolume volume)
    {
        return new Candle(timestamp, open, high, low, close, volume);
    }
    
    public void Update(MarketPrice price, TradingVolume volume)
    {
        if (price > High) High = price;
        if (price < Low) Low = price;
        Close = price;
        Volume = Volume.Add(volume);
    }
    
    public decimal GetBodySize() => Math.Abs((Close.Value - Open.Value) / Open.Value);
    public decimal GetWickSize() => (High.Value - Low.Value) / Open.Value;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Timestamp;
        yield return Open;
        yield return High;
        yield return Low;
        yield return Close;
        yield return Volume;
    }
}

/// <summary>지표 값</summary>
public sealed class IndicatorValue : ValueObject
{
    public decimal Value { get; }
    public DateTime CalculatedAt { get; }
    
    public IndicatorValue(decimal value, DateTime calculatedAt)
    {
        Value = value;
        CalculatedAt = calculatedAt;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CalculatedAt;
    }
}

/// <summary>지표 파라미터</summary>
public sealed class IndicatorParameters : ValueObject
{
    public Dictionary<string, object> Values { get; }
    
    public IndicatorParameters(params (string key, object value)[] parameters)
    {
        Values = new Dictionary<string, object>();
        foreach (var (key, value) in parameters)
        {
            Values[key] = value;
        }
    }
    
    public T Get<T>(string key, T defaultValue = default)
    {
        return Values.TryGetValue(key, out var value) ? (T)value : defaultValue;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var kvp in Values)
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }
}

/// <summary>차트 주기</summary>
public sealed class ChartInterval : ValueObject
{
    public IntervalType Type { get; }
    public int Value { get; } // 분단위 또는 기간
    
    public ChartInterval(IntervalType type, int value = 1)
    {
        Type = type;
        Value = value;
    }
    
    public bool IsInSameInterval(DateTime time1, DateTime time2)
    {
        return Type switch
        {
            IntervalType.Minute =>
                time1.Date == time2.Date &&
                time1.Hour == time2.Hour &&
                time1.Minute / Value == time2.Minute / Value,
            
            IntervalType.Hour =>
                time1.Date == time2.Date &&
                time1.Hour / Value == time2.Hour / Value,
            
            IntervalType.Day =>
                time1.Date == time2.Date,
            
            IntervalType.Week =>
                GetWeekOfYear(time1) == GetWeekOfYear(time2),
            
            IntervalType.Month =>
                time1.Year == time2.Year && time1.Month == time2.Month,
            
            _ => false
        };
    }
    
    public DateTime GetCandleStartTime(DateTime dateTime)
    {
        return Type switch
        {
            IntervalType.Minute =>
                new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                    dateTime.Hour, (dateTime.Minute / Value) * Value, 0),
            
            IntervalType.Hour =>
                new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                    (dateTime.Hour / Value) * Value, 0, 0),
            
            IntervalType.Day =>
                new DateTime(dateTime.Year, dateTime.Month, dateTime.Day),
            
            IntervalType.Week =>
                new DateTime(dateTime.Year, dateTime.Month, dateTime.Day)
                    .AddDays(-(int)dateTime.DayOfWeek),
            
            IntervalType.Month =>
                new DateTime(dateTime.Year, dateTime.Month, 1),
            
            _ => dateTime
        };
    }
    
    private static int GetWeekOfYear(DateTime dateTime)
    {
        var cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
        var calendar = cultureInfo.Calendar;
        return calendar.GetWeekOfYear(dateTime,
            cultureInfo.DateTimeFormat.CalendarWeekRule,
            cultureInfo.DateTimeFormat.FirstDayOfWeek);
    }
    
    public override string ToString() => $"{Type}({Value})";
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
    }
}

/// <summary>지표 신호</summary>
public sealed class IndicatorSignal : ValueObject
{
    public SignalType Type { get; }
    public decimal Value { get; }
    public string Description { get; }
    public DateTime GeneratedAt { get; }
    public decimal Strength { get; } // 0~1
    
    public IndicatorSignal(SignalType type, decimal value, string description, DateTime generatedAt)
    {
        Type = type;
        Value = value;
        Description = description;
        GeneratedAt = generatedAt;
        Strength = 0.5m;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
        yield return GeneratedAt;
    }
}

/// <summary>체결 데이터</summary>
public sealed class Execution : ValueObject
{
    public MarketPrice Price { get; }
    public TradingVolume Volume { get; }
    public DateTime ExecutionTime { get; }
    public ExecutionType Type { get; }
    
    public Execution(
        MarketPrice price,
        TradingVolume volume,
        DateTime executionTime,
        ExecutionType type)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Volume = volume ?? throw new ArgumentNullException(nameof(volume));
        ExecutionTime = executionTime;
        Type = type;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Price;
        yield return Volume;
        yield return ExecutionTime;
        yield return Type;
    }
}
```

### 2.3 Entities

```csharp
/// <summary>차트 패턴</summary>
public class ChartPattern : Entity<ChartPatternId>
{
    public PatternType Type { get; init; }
    public PatternStrength Strength { get; init; }
    public DateTime DetectedAt { get; init; }
    public string Description { get; init; }
    public List<int> CandleIndices { get; init; } = new();
}

/// <summary>지지선</summary>
public class SupportLevel : Entity<SupportLevelId>
{
    public MarketPrice Price { get; init; }
    public int TouchCount { get; init; }
    public DateTime IdentifiedAt { get; init; }
}

/// <summary>저항선</summary>
public class ResistanceLevel : Entity<ResistanceLevelId>
{
    public MarketPrice Price { get; init; }
    public int TouchCount { get; init; }
    public DateTime IdentifiedAt { get; init; }
}

/// <summary>패턴 후보</summary>
public class PatternCandidate : Entity<PatternCandidateId>
{
    public ChartPattern Pattern { get; init; }
    public decimal Confidence { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>거래량 프로필</summary>
public class VolumeProfile : Entity<VolumeProfileId>
{
    public MarketPrice PriceLevel { get; init; }
    public TradingVolume Volume { get; init; }
    public decimal Percentage { get; init; }
    public DateTime RecordedAt { get; init; }
}
```

### 2.4 Enumerations

```csharp
/// <summary>장 상태</summary>
public enum MarketStatus
{
    PreOpen = 1,        // 장 시작 전
    Opening = 2,        // 시가 결정
    Trading = 3,        // 정규장
    Closing = 4,        // 종가 결정
    Closed = 5,         // 장 마감
    Halted = 6          // 거래 정지
}

/// <summary>차트 주기 유형</summary>
public enum IntervalType
{
    Minute = 1,
    Hour = 2,
    Day = 3,
    Week = 4,
    Month = 5,
    Tick = 6
}

/// <summary>기술적 지표 유형</summary>
public enum IndicatorType
{
    // 추세 지표
    SMA = 1,            // 단순 이동평균
    EMA = 2,            // 지수 이동평균
    MACD = 3,           // MACD
    ADX = 4,            // 평균 방향성 지수
    
    // 모멘텀 지표
    RSI = 5,            // 상대 강도 지수
    Stochastic = 6,     // 스토캐스틱
    CCI = 7,            // 상품 채널 지수
    
    // 변동성 지표
    BollingerBands = 8, // 볼린저 밴드
    ATR = 9,            // 평균 진폭
    
    // 거래량 지표
    OBV = 10,           // 누적 거래량
    MFI = 11,           // 자금 흐름 지수
    VWAP = 12           // 거래량 가중 평균가
}

/// <summary>신호 유형</summary>
public enum SignalType
{
    Neutral = 0,
    BullishCross = 1,   // 상승 크로스
    BearishCross = 2,   // 하락 크로스
    Overbought = 3,     // 과매수
    Oversold = 4,       // 과매도
    Divergence = 5,     // 다이버전스
    UpperBandTouch = 6, // 상단 밴드 터치
    LowerBandTouch = 7  // 하단 밴드 터치
}

/// <summary>알림 유형</summary>
public enum AlertType
{
    SurgeAlert = 1,     // 급등
    PlungeAlert = 2,    // 급락
    VolumeAlert = 3,    // 거래량 급증
    VolatilityAlert = 4 // 변동성 증가
}

/// <summary>주문 방향</summary>
public enum OrderSide
{
    Buy = 1,
    Sell = 2
}

/// <summary>체결 유형</summary>
public enum ExecutionType
{
    Buy = 1,
    Sell = 2,
    Unknown = 3
}

/// <summary>방향</summary>
public enum ChangeDirection
{
    Up = 1,
    Down = 2,
    Flat = 3
}

/// <summary>호가 불균형 유형</summary>
public enum ImbalanceType
{
    BidHeavy = 1,       // 매수 우세
    AskHeavy = 2,       // 매도 우세
    Balanced = 3        // 균형
}

/// <summary>추세 유형</summary>
public enum Trend
{
    Uptrend = 1,
    Downtrend = 2,
    Sideways = 3
}

/// <summary>패턴 유형</summary>
public enum PatternType
{
    HeadAndShoulders = 1,
    TriangleConvergence = 2,
    DoubleTop = 3,
    DoubleBottom = 4,
    CupAndHandle = 5,
    FlagPattern = 6,
    WedgePattern = 7
}

/// <summary>패턴 강도</summary>
public enum PatternStrength
{
    Weak = 1,
    Medium = 2,
    Strong = 3
}

/// <summary>가격 제한 유형</summary>
public enum PriceLimitType
{
    UpperLimit = 1,
    LowerLimit = 2
}
```

---

## 3. Business Rules & Invariants

### 3.1 핵심 불변식 (Invariants)

```csharp
/// <summary>
/// 시장 데이터의 핵심 비즈니스 규칙을 검증하는 클래스
/// </summary>
public static class MarketDataInvariants
{
    /// <summary>
    /// Invariant 1: 고가 >= 저가
    /// </summary>
    public static void ValidateHighLowPrices(MarketData marketData)
    {
        if (marketData.HighPrice != null && marketData.LowPrice != null)
        {
            if (marketData.HighPrice < marketData.LowPrice)
            {
                throw new InvalidMarketDataException(
                    "High price must be greater than or equal to low price");
            }
        }
    }
    
    /// <summary>
    /// Invariant 2: 현재가는 고가/저가 범위 내
    /// </summary>
    public static void ValidateCurrentPriceWithinRange(MarketData marketData)
    {
        if (marketData.HighPrice != null && marketData.LowPrice != null &&
            marketData.CurrentPrice != null)
        {
            if (marketData.CurrentPrice > marketData.HighPrice ||
                marketData.CurrentPrice < marketData.LowPrice)
            {
                throw new InvalidMarketDataException(
                    "Current price must be within high-low range");
            }
        }
    }
    
    /// <summary>
    /// Invariant 3: 거래량은 음수가 될 수 없음
    /// </summary>
    public static void ValidateVolume(TradingVolume volume)
    {
        if (volume.Value < 0)
        {
            throw new InvalidMarketDataException(
                "Volume cannot be negative");
        }
    }
    
    /// <summary>
    /// Invariant 4: 호가는 거래량을 가져야 함
    /// </summary>
    public static void ValidateOrderBook(List<PriceLevel> orderBook)
    {
        if (orderBook.Any(level => level.Volume.Value < 0))
        {
            throw new InvalidMarketDataException(
                "Order book volume cannot be negative");
        }
    }
}
```

### 3.2 비즈니스 규칙

```csharp
/// <summary>
/// 시장 데이터 관련 비즈니스 규칙
/// </summary>
public static class MarketDataBusinessRules
{
    /// <summary>
    /// 규칙 1: 가격 조회 가능 확인
    /// 장이 열려있거나 종가 결정 시기에만 조회 가능
    /// </summary>
    public static bool IsPriceQueryable(MarketData marketData)
    {
        return marketData.Status == MarketStatus.Trading ||
               marketData.Status == MarketStatus.Closing ||
               marketData.Status == MarketStatus.Closed;
    }
    
    /// <summary>
    /// 규칙 2: 실시간 데이터 신뢰도 확인
    /// 신뢰도가 80% 이상일 때만 사용
    /// </summary>
    public static bool IsRealtimeDataReliable(MarketData marketData)
    {
        return marketData.DataReliability >= 0.8m;
    }
    
    /// <summary>
    /// 규칙 3: 호가 스프레드 체크
    /// 스프레드가 너무 크면 이상 신호
    /// </summary>
    public static bool IsSpreadAbnormal(MarketData marketData, decimal threshold = 0.05m)
    {
        return marketData.Spread > threshold;
    }
    
    /// <summary>
    /// 규칙 4: 거래량 급증 감지
    /// 평균 거래량의 3배 이상
    /// </summary>
    public static bool IsVolumeAnomalous(
        TradingVolume currentVolume,
        decimal averageVolume,
        decimal threshold = 3m)
    {
        return averageVolume > 0 && currentVolume.Value / averageVolume > threshold;
    }
    
    /// <summary>
    /// 규칙 5: 차트 패턴 신뢰도 검증
    /// 패턴의 강도와 신뢰도에 따라 사용 결정
    /// </summary>
    public static bool IsPatternReliable(
        ChartPattern pattern,
        PatternStrength minimumStrength = PatternStrength.Medium)
    {
        return pattern.Strength >= minimumStrength;
    }
    
    /// <summary>
    /// 규칙 6: 데이터 최신성 확인
    /// 데이터가 1초 이내의 최신 데이터인지 확인
    /// </summary>
    public static bool IsDataCurrent(MarketData marketData, int maxAgeSeconds = 1)
    {
        var age = DateTime.UtcNow - marketData.LastUpdatedAt;
        return age.TotalSeconds <= maxAgeSeconds;
    }
}
```

---

## 4. Domain Events

```csharp
/// <summary>
/// 가격 업데이트 이벤트
/// 종목의 현재가가 변경되었을 때 발행
/// </summary>
public sealed class PriceUpdatedEvent : DomainEvent
{
    public MarketDataId MarketDataId { get; }
    public StockCode StockCode { get; }
    public MarketPrice CurrentPrice { get; }
    public MarketPrice PreviousPrice { get; }
    public TradingVolume Volume { get; }
    public DateTime UpdatedAt { get; }
    
    public PriceUpdatedEvent(
        MarketDataId marketDataId,
        StockCode stockCode,
        MarketPrice currentPrice,
        MarketPrice previousPrice,
        TradingVolume volume,
        DateTime updatedAt)
    {
        MarketDataId = marketDataId;
        StockCode = stockCode;
        CurrentPrice = currentPrice;
        PreviousPrice = previousPrice;
        Volume = volume;
        UpdatedAt = updatedAt;
    }
}

/// <summary>
/// 가격 극단 이벤트 (급등/급락)
/// 급등 또는 급락이 감지되었을 때 발행
/// </summary>
public sealed class PriceExtremeEvent : DomainEvent
{
    public MarketDataId MarketDataId { get; }
    public StockCode StockCode { get; }
    public MarketPrice CurrentPrice { get; }
    public PriceChange PriceChange { get; }
    public AlertType AlertType { get; }
    public DateTime DetectedAt { get; }
    
    public PriceExtremeEvent(
        MarketDataId marketDataId,
        StockCode stockCode,
        MarketPrice currentPrice,
        PriceChange priceChange,
        AlertType alertType,
        DateTime detectedAt)
    {
        MarketDataId = marketDataId;
        StockCode = stockCode;
        CurrentPrice = currentPrice;
        PriceChange = priceChange;
        AlertType = alertType;
        DetectedAt = detectedAt;
    }
}

/// <summary>
/// 호가 불균형 이벤트
/// 매수/매도 호가의 심각한 불균형이 감지되었을 때 발행
/// </summary>
public sealed class OrderBookImbalanceEvent : DomainEvent
{
    public MarketDataId MarketDataId { get; }
    public StockCode StockCode { get; }
    public decimal BidAskRatio { get; }
    public ImbalanceType ImbalanceType { get; }
    public DateTime DetectedAt { get; }
    
    public OrderBookImbalanceEvent(
        MarketDataId marketDataId,
        StockCode stockCode,
        decimal bidAskRatio,
        ImbalanceType imbalanceType,
        DateTime detectedAt)
    {
        MarketDataId = marketDataId;
        StockCode = stockCode;
        BidAskRatio = bidAskRatio;
        ImbalanceType = imbalanceType;
        DetectedAt = detectedAt;
    }
}

/// <summary>
/// 대량 체결 이벤트
/// 10,000주 이상의 대량 체결이 발생했을 때 발행
/// </summary>
public sealed class LargeExecutionEvent : DomainEvent
{
    public MarketDataId MarketDataId { get; }
    public StockCode StockCode { get; }
    public MarketPrice ExecutionPrice { get; }
    public TradingVolume ExecutionVolume { get; }
    public DateTime ExecutionTime { get; }
    
    public LargeExecutionEvent(
        MarketDataId marketDataId,
        StockCode stockCode,
        MarketPrice executionPrice,
        TradingVolume executionVolume,
        DateTime executionTime)
    {
        MarketDataId = marketDataId;
        StockCode = stockCode;
        ExecutionPrice = executionPrice;
        ExecutionVolume = executionVolume;
        ExecutionTime = executionTime;
    }
}

/// <summary>
/// 거래량 이상 이벤트
/// 거래량이 평균의 3배 이상 급증했을 때 발행
/// </summary>
public sealed class VolumeAnomalyEvent : DomainEvent
{
    public MarketDataId MarketDataId { get; }
    public StockCode StockCode { get; }
    public TradingVolume CurrentVolume { get; }
    public decimal VolumeIncreaseRate { get; } // 퍼센트
    public DateTime DetectedAt { get; }
    
    public VolumeAnomalyEvent(
        MarketDataId marketDataId,
        StockCode stockCode,
        TradingVolume currentVolume,
        decimal volumeIncreaseRate,
        DateTime detectedAt)
    {
        MarketDataId = marketDataId;
        StockCode = stockCode;
        CurrentVolume = currentVolume;
        VolumeIncreaseRate = volumeIncreaseRate;
        DetectedAt = detectedAt;
    }
}

/// <summary>
/// 장 상태 변경 이벤트
/// 거래 시간이 변경되었을 때 발행 (개시/정규장/마감)
/// </summary>
public sealed class MarketStatusChangedEvent : DomainEvent
{
    public MarketDataId MarketDataId { get; }
    public StockCode StockCode { get; }
    public MarketStatus PreviousStatus { get; }
    public MarketStatus NewStatus { get; }
    public DateTime ChangedAt { get; }
    
    public MarketStatusChangedEvent(
        MarketDataId marketDataId,
        StockCode stockCode,
        MarketStatus previousStatus,
        MarketStatus newStatus,
        DateTime changedAt)
    {
        MarketDataId = marketDataId;
        StockCode = stockCode;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedAt = changedAt;
    }
}

/// <summary>
/// 가격 제한 이벤트
/// 상한가 또는 하한가 진입 감지
/// </summary>
public sealed class PriceLimitEvent : DomainEvent
{
    public MarketDataId MarketDataId { get; }
    public StockCode StockCode { get; }
    public MarketPrice Price { get; }
    public PriceLimitType LimitType { get; }
    public DateTime DetectedAt { get; }
    
    public PriceLimitEvent(
        MarketDataId marketDataId,
        StockCode stockCode,
        MarketPrice price,
        PriceLimitType limitType,
        DateTime detectedAt)
    {
        MarketDataId = marketDataId;
        StockCode = stockCode;
        Price = price;
        LimitType = limitType;
        DetectedAt = detectedAt;
    }
}

/// <summary>
/// 캔들 추가 이벤트
/// 차트에 새로운 캔들이 추가되었을 때 발행
/// </summary>
public sealed class CandleAddedEvent : DomainEvent
{
    public ChartDataId ChartDataId { get; }
    public StockCode StockCode { get; }
    public ChartInterval Interval { get; }
    public Candle Candle { get; }
    public DateTime AddedAt { get; }
    
    public CandleAddedEvent(
        ChartDataId chartDataId,
        StockCode stockCode,
        ChartInterval interval,
        Candle candle,
        DateTime addedAt)
    {
        ChartDataId = chartDataId;
        StockCode = stockCode;
        Interval = interval;
        Candle = candle;
        AddedAt = addedAt;
    }
}

/// <summary>
/// 패턴 감지 이벤트
/// 차트 패턴이 감지되었을 때 발행
/// </summary>
public sealed class PatternDetectedEvent : DomainEvent
{
    public ChartDataId ChartDataId { get; }
    public StockCode StockCode { get; }
    public ChartInterval Interval { get; }
    public ChartPattern Pattern { get; }
    public DateTime DetectedAt { get; }
    
    public PatternDetectedEvent(
        ChartDataId chartDataId,
        StockCode stockCode,
        ChartInterval interval,
        ChartPattern pattern,
        DateTime detectedAt)
    {
        ChartDataId = chartDataId;
        StockCode = stockCode;
        Interval = interval;
        Pattern = pattern;
        DetectedAt = detectedAt;
    }
}

/// <summary>
/// 지표 신호 생성 이벤트
/// 기술적 지표에서 매매 신호가 생성되었을 때 발행
/// </summary>
public sealed class IndicatorSignalGeneratedEvent : DomainEvent
{
    public TechnicalIndicatorId IndicatorId { get; }
    public StockCode StockCode { get; }
    public IndicatorType IndicatorType { get; }
    public ChartInterval Interval { get; }
    public SignalType SignalType { get; }
    public decimal Value { get; }
    public DateTime GeneratedAt { get; }
    
    public IndicatorSignalGeneratedEvent(
        TechnicalIndicatorId indicatorId,
        StockCode stockCode,
        IndicatorType indicatorType,
        ChartInterval interval,
        SignalType signalType,
        decimal value,
        DateTime generatedAt)
    {
        IndicatorId = indicatorId;
        StockCode = stockCode;
        IndicatorType = indicatorType;
        Interval = interval;
        SignalType = signalType;
        Value = value;
        GeneratedAt = generatedAt;
    }
}
```

---

## 5. Application Services

```csharp
/// <summary>
/// 시장 데이터 애플리케이션 서비스
/// 시장 데이터 관리 관련 비즈니스 프로세스 오케스트레이션
/// </summary>
public sealed class MarketDataService : IApplicationService
{
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly IChartDataRepository _chartDataRepository;
    private readonly ITechnicalIndicatorRepository _indicatorRepository;
    private readonly IKiwoomApiService _kiwoomApi;
    private readonly IIndicatorCalculationService _indicatorService;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarketDataService> _logger;
    
    public MarketDataService(
        IMarketDataRepository marketDataRepository,
        IChartDataRepository chartDataRepository,
        ITechnicalIndicatorRepository indicatorRepository,
        IKiwoomApiService kiwoomApi,
        IIndicatorCalculationService indicatorService,
        IEventBus eventBus,
        IUnitOfWork unitOfWork,
        ILogger<MarketDataService> logger)
    {
        _marketDataRepository = marketDataRepository;
        _chartDataRepository = chartDataRepository;
        _indicatorRepository = indicatorRepository;
        _kiwoomApi = kiwoomApi;
        _indicatorService = indicatorService;
        _eventBus = eventBus;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    /// <summary>
    /// 실시간 시세 수신 시작
    /// </summary>
    public async Task<string> StartRealtimePriceReceptionAsync(StockCode stockCode)
    {
        try
        {
            _logger.LogInformation($"Starting real-time reception for {stockCode}");
            
            // 시장 데이터 생성 또는 조회
            var marketData = await _marketDataRepository.GetByStockCodeAsync(stockCode);
            if (marketData == null)
            {
                // 이전 종가 조회
                var previousClose = await _kiwoomApi.GetPreviousCloseAsync(stockCode);
                marketData = MarketData.Create(stockCode, previousClose);
                await _marketDataRepository.AddAsync(marketData);
            }
            
            // 키움 API에서 실시간 수신 시작
            var subscriptionId = await _kiwoomApi.SubscribeToRealtimePriceAsync(
                stockCode.Value,
                async (priceData) =>
                {
                    await HandleRealtimePriceUpdateAsync(marketData, priceData);
                });
            
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation($"Real-time reception started for {stockCode}");
            return subscriptionId;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting real-time reception: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 실시간 가격 업데이트 처리
    /// </summary>
    private async Task HandleRealtimePriceUpdateAsync(
        MarketData marketData,
        RealtimePriceData priceData)
    {
        try
        {
            var currentPrice = new MarketPrice(priceData.CurrentPrice);
            var volume = new TradingVolume(priceData.Volume);
            var amount = new Money(priceData.Amount);
            
            // 가격 업데이트
            marketData.UpdateCurrentPrice(currentPrice, volume, amount, DateTime.UtcNow);
            
            // 호가 업데이트
            if (priceData.OrderBook != null)
            {
                var bidLevels = priceData.OrderBook.BidLevels
                    .Select(b => new PriceLevel(
                        new MarketPrice(b.Price),
                        new TradingVolume(b.Volume),
                        b.OrderCount,
                        OrderSide.Buy))
                    .ToList();
                
                var askLevels = priceData.OrderBook.AskLevels
                    .Select(a => new PriceLevel(
                        new MarketPrice(a.Price),
                        new TradingVolume(a.Volume),
                        a.OrderCount,
                        OrderSide.Sell))
                    .ToList();
                
                marketData.UpdateOrderBook(bidLevels, askLevels, DateTime.UtcNow);
            }
            
            // 저장
            await _marketDataRepository.UpdateAsync(marketData);
            
            // 이벤트 발행
            foreach (var @event in marketData.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error handling price update: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 차트 데이터 생성
    /// </summary>
    public async Task<ChartDataDto> GenerateChartDataAsync(
        StockCode stockCode,
        ChartInterval interval,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var chartData = ChartData.Create(stockCode, interval);
            
            // 캔들 데이터 조회
            var candles = await _kiwoomApi.GetCandleDataAsync(
                stockCode.Value,
                interval,
                startDate,
                endDate);
            
            // 캔들 추가
            foreach (var candle in candles)
            {
                chartData.AddCandle(candle);
            }
            
            // 저장
            await _chartDataRepository.AddAsync(chartData);
            await _unitOfWork.CommitAsync();
            
            // 이벤트 발행
            foreach (var @event in chartData.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return new ChartDataDto
            {
                ChartDataId = chartData.Id,
                StockCode = chartData.StockCode.Value,
                Interval = interval.ToString(),
                CandleCount = chartData.CandleCount,
                Candles = chartData.Candles.Select(c => new CandleDto
                {
                    Timestamp = c.Timestamp,
                    Open = c.Open.Value,
                    High = c.High.Value,
                    Low = c.Low.Value,
                    Close = c.Close.Value,
                    Volume = c.Volume.Value
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating chart data: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 기술적 지표 계산
    /// </summary>
    public async Task<IndicatorDto> CalculateIndicatorAsync(
        StockCode stockCode,
        IndicatorType indicatorType,
        ChartInterval interval)
    {
        try
        {
            // 차트 데이터 조회
            var chartData = await _chartDataRepository.GetByStockCodeAndIntervalAsync(
                stockCode, interval);
            
            if (chartData == null || chartData.CandleCount == 0)
                throw new NoDataException("No chart data available for indicator calculation");
            
            // 지표 생성
            var indicator = TechnicalIndicator.Create(
                stockCode,
                indicatorType,
                interval,
                new IndicatorParameters());
            
            // 지표 계산
            switch (indicatorType)
            {
                case IndicatorType.RSI:
                    indicator.CalculateRSI(chartData.Candles.ToList());
                    break;
                
                case IndicatorType.MACD:
                    indicator.CalculateMACD(chartData.Candles.ToList());
                    break;
                
                case IndicatorType.BollingerBands:
                    indicator.CalculateBollingerBands(chartData.Candles.ToList());
                    break;
                
                default:
                    throw new NotSupportedException($"Indicator {indicatorType} not supported");
            }
            
            // 저장
            await _indicatorRepository.AddAsync(indicator);
            await _unitOfWork.CommitAsync();
            
            // 이벤트 발행
            foreach (var @event in indicator.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return new IndicatorDto
            {
                IndicatorId = indicator.Id,
                StockCode = indicator.StockCode.Value,
                IndicatorType = indicator.Type.ToString(),
                Interval = interval.ToString(),
                CurrentSignal = indicator.CurrentSignal.ToString(),
                Values = indicator.Values.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Value)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calculating indicator: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// 기술적 지표 계산 서비스
/// 복잡한 지표 계산 담당
/// </summary>
public sealed class IndicatorCalculationService : IIndicatorCalculationService
{
    private readonly ILogger<IndicatorCalculationService> _logger;
    
    public IndicatorCalculationService(ILogger<IndicatorCalculationService> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// 지수 이동평균 (EMA) 계산
    /// </summary>
    public List<decimal> CalculateEMA(List<decimal> values, int period)
    {
        if (values.Count < period)
            throw new InsufficientDataException($"Need at least {period} values");
        
        var emaValues = new List<decimal>();
        var multiplier = 2.0m / (period + 1);
        
        // 첫 SMA 계산
        var sma = values.Take(period).Average();
        emaValues.Add(sma);
        
        // EMA 계산
        for (int i = period; i < values.Count; i++)
        {
            var ema = (values[i] - emaValues.Last()) * multiplier + emaValues.Last();
            emaValues.Add(ema);
        }
        
        return emaValues;
    }
    
    /// <summary>
    /// 단순 이동평균 (SMA) 계산
    /// </summary>
    public List<decimal> CalculateSMA(List<decimal> values, int period)
    {
        if (values.Count < period)
            throw new InsufficientDataException($"Need at least {period} values");
        
        var smaValues = new List<decimal>();
        
        for (int i = period - 1; i < values.Count; i++)
        {
            var sum = values.Skip(i - period + 1).Take(period).Sum();
            smaValues.Add(sum / period);
        }
        
        return smaValues;
    }
}
```

---

## 6. Infrastructure Layer

```csharp
/// <summary>
/// 시장 데이터 저장소 구현
/// </summary>
public sealed class MarketDataRepository : IMarketDataRepository
{
    private readonly MarketDataDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MarketDataRepository> _logger;
    
    public MarketDataRepository(
        MarketDataDbContext context,
        IDistributedCache cache,
        ILogger<MarketDataRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    /// <summary>
    /// 종목 코드로 시장 데이터 조회
    /// </summary>
    public async Task<MarketData> GetByStockCodeAsync(StockCode stockCode)
    {
        var cacheKey = $"marketdata:{stockCode.Value}";
        
        // 캐시 확인
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<MarketData>(cachedData);
        }
        
        // DB 조회
        var marketData = await _context.MarketDatas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.StockCode == stockCode);
        
        if (marketData != null)
        {
            // 캐시 저장 (10초 TTL - 실시간 업데이트)
            var json = JsonSerializer.Serialize(marketData);
            await _cache.SetStringAsync(cacheKey, json, TimeSpan.FromSeconds(10));
        }
        
        return marketData;
    }
    
    /// <summary>
    /// 시장 데이터 저장
    /// </summary>
    public async Task AddAsync(MarketData marketData)
    {
        _context.MarketDatas.Add(marketData);
        await _context.SaveChangesAsync();
        await InvalidateCacheAsync(marketData.StockCode);
        
        _logger.LogInformation($"Market data added for {marketData.StockCode}");
    }
    
    /// <summary>
    /// 시장 데이터 업데이트
    /// </summary>
    public async Task UpdateAsync(MarketData marketData)
    {
        _context.MarketDatas.Update(marketData);
        await _context.SaveChangesAsync();
        await InvalidateCacheAsync(marketData.StockCode);
    }
    
    private async Task InvalidateCacheAsync(StockCode stockCode)
    {
        var cacheKey = $"marketdata:{stockCode.Value}";
        await _cache.RemoveAsync(cacheKey);
    }
}

/// <summary>
/// 키움 API 시장 데이터 어댑터
/// </summary>
public sealed class KiwoomMarketDataAdapter : IMarketDataProvider
{
    private readonly IKiwoomApi _kiwoomApi;
    private readonly ILogger<KiwoomMarketDataAdapter> _logger;
    
    public KiwoomMarketDataAdapter(
        IKiwoomApi kiwoomApi,
        ILogger<KiwoomMarketDataAdapter> logger)
    {
        _kiwoomApi = kiwoomApi;
        _logger = logger;
    }
    
    /// <summary>
    /// 실시간 시세 구독
    /// </summary>
    public async Task<string> SubscribeToRealtimePriceAsync(
        string stockCode,
        Func<RealtimePriceData, Task> onPriceUpdate)
    {
        try
        {
            return await _kiwoomApi.SubscribeToRealtimePriceAsync(stockCode, onPriceUpdate);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error subscribing to real-time price: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 캔들 데이터 조회
    /// </summary>
    public async Task<List<Candle>> GetCandleDataAsync(
        string stockCode,
        ChartInterval interval,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var candleData = await _kiwoomApi.GetCandleDataAsync(
                stockCode,
                interval,
                startDate,
                endDate);
            
            return candleData
                .Select(c => Candle.Create(
                    c.Timestamp,
                    new MarketPrice(c.Open),
                    new MarketPrice(c.High),
                    new MarketPrice(c.Low),
                    new MarketPrice(c.Close),
                    new TradingVolume(c.Volume)))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting candle data: {ex.Message}");
            throw;
        }
    }
}
```

---

## 7. API/Interface Specifications

```csharp
/// <summary>
/// 시장 데이터 API
/// </summary>
[ApiController]
[Route("api/market-data")]
[Authorize]
public class MarketDataController : ControllerBase
{
    private readonly IMediator _mediator;
    
    /// <summary>
    /// GET /api/market-data/{stockCode}
    /// 현재 시세 조회
    /// </summary>
    [HttpGet("{stockCode}")]
    public async Task<ActionResult<MarketDataDto>> GetMarketDataAsync(string stockCode)
    {
        var query = new GetMarketDataQuery(new StockCode(stockCode));
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    /// <summary>
    /// POST /api/market-data/{stockCode}/subscribe
    /// 실시간 시세 수신 시작
    /// </summary>
    [HttpPost("{stockCode}/subscribe")]
    public async Task<ActionResult> SubscribeRealtimePriceAsync(string stockCode)
    {
        var command = new SubscribeRealtimePriceCommand(new StockCode(stockCode));
        await _mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// GET /api/market-data/{stockCode}/chart
    /// 차트 데이터 조회
    /// </summary>
    [HttpGet("{stockCode}/chart")]
    public async Task<ActionResult<ChartDataDto>> GetChartDataAsync(
        string stockCode,
        [FromQuery] string interval = "D",
        [FromQuery] int days = 30)
    {
        var query = new GetChartDataQuery(
            new StockCode(stockCode),
            interval,
            days);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    /// <summary>
    /// GET /api/market-data/{stockCode}/indicators
    /// 기술적 지표 조회
    /// </summary>
    [HttpGet("{stockCode}/indicators")]
    public async Task<ActionResult<List<IndicatorDto>>> GetIndicatorsAsync(
        string stockCode,
        [FromQuery] string indicatorTypes = "RSI,MACD")
    {
        var query = new GetIndicatorsQuery(
            new StockCode(stockCode),
            indicatorTypes.Split(','));
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

/// <summary>
/// 시장 데이터 저장소 인터페이스
/// </summary>
public interface IMarketDataRepository
{
    Task<MarketData> GetByStockCodeAsync(StockCode stockCode);
    Task<IEnumerable<MarketData>> GetByStockCodesAsync(IEnumerable<StockCode> stockCodes);
    Task AddAsync(MarketData marketData);
    Task UpdateAsync(MarketData marketData);
    Task DeleteAsync(StockCode stockCode);
}

/// <summary>
/// 차트 데이터 저장소 인터페이스
/// </summary>
public interface IChartDataRepository
{
    Task<ChartData> GetByStockCodeAndIntervalAsync(StockCode stockCode, ChartInterval interval);
    Task<IEnumerable<Candle>> GetCandlesAsync(StockCode stockCode, ChartInterval interval, int count);
    Task AddAsync(ChartData chartData);
    Task UpdateAsync(ChartData chartData);
}

/// <summary>
/// 기술적 지표 저장소 인터페이스
/// </summary>
public interface ITechnicalIndicatorRepository
{
    Task<TechnicalIndicator> GetByStockCodeAndTypeAsync(StockCode stockCode, IndicatorType type, ChartInterval interval);
    Task AddAsync(TechnicalIndicator indicator);
    Task UpdateAsync(TechnicalIndicator indicator);
}

/// <summary>
/// 시장 데이터 제공자 인터페이스
/// </summary>
public interface IMarketDataProvider
{
    Task<string> SubscribeToRealtimePriceAsync(string stockCode, Func<RealtimePriceData, Task> onPriceUpdate);
    Task<List<Candle>> GetCandleDataAsync(string stockCode, ChartInterval interval, DateTime startDate, DateTime endDate);
    Task<MarketPrice> GetPreviousCloseAsync(StockCode stockCode);
}

/// <summary>
/// 기술적 지표 계산 서비스 인터페이스
/// </summary>
public interface IIndicatorCalculationService
{
    List<decimal> CalculateEMA(List<decimal> values, int period);
    List<decimal> CalculateSMA(List<decimal> values, int period);
}
```

---

## 8. Error Handling & Validation

```csharp
/// <summary>
/// 시장 데이터 관련 사용자 정의 예외
/// </summary>

/// <summary>유효하지 않은 시장 데이터</summary>
public sealed class InvalidMarketDataException : DomainException
{
    public InvalidMarketDataException(string message) : base(message) { }
}

/// <summary>중복된 캔들 데이터</summary>
public sealed class DuplicateCandleException : DomainException
{
    public DuplicateCandleException(string message) : base(message) { }
}

/// <summary>데이터 부족</summary>
public sealed class InsufficientDataException : DomainException
{
    public InsufficientDataException(string message) : base(message) { }
}

/// <summary>데이터 없음</summary>
public sealed class NoDataException : DomainException
{
    public NoDataException(string message) : base(message) { }
}

/// <summary>시장 데이터 검증</summary>
public sealed class MarketDataValidation
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();
    
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();
    public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
    public bool IsValid => _errors.Count == 0;
    
    public void AddError(string message) { if (!string.IsNullOrWhiteSpace(message)) _errors.Add(message); }
    public void AddWarning(string message) { if (!string.IsNullOrWhiteSpace(message)) _warnings.Add(message); }
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
    Task<string> SubscribeToRealtimePriceAsync(string stockCode, Func<RealtimePriceData, Task> onPriceUpdate);
    Task<List<CandleData>> GetCandleDataAsync(string stockCode, ChartInterval interval, DateTime startDate, DateTime endDate);
    Task<MarketPrice> GetPreviousCloseAsync(StockCode stockCode);
    Task UnsubscribeFromRealtimePriceAsync(string subscriptionId);
}

/// <summary>
/// 실시간 가격 데이터
/// </summary>
public record RealtimePriceData
{
    public decimal CurrentPrice { get; init; }
    public long Volume { get; init; }
    public decimal Amount { get; init; }
    public OrderBookData OrderBook { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// 호가 데이터
/// </summary>
public record OrderBookData
{
    public List<LevelData> BidLevels { get; init; }
    public List<LevelData> AskLevels { get; init; }
}

/// <summary>
/// 호가 수준 데이터
/// </summary>
public record LevelData
{
    public decimal Price { get; init; }
    public long Volume { get; init; }
    public int OrderCount { get; init; }
}

/// <summary>
/// 캔들 데이터
/// </summary>
public record CandleData
{
    public DateTime Timestamp { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public long Volume { get; init; }
}
```

---

## 10. Performance Considerations

### 10.1 캐싱 전략

```csharp
/// <summary>
/// 시장 데이터 캐싱 서비스
/// </summary>
public sealed class MarketDataCachingService
{
    private readonly IDistributedCache _cache;
    private readonly IMarketDataRepository _repository;
    
    /// <summary>
    /// 다층 캐싱:
    /// - L1: Redis (10초 TTL) - 실시간 데이터
    /// - L2: Database - 영구 저장소
    /// </summary>
    public async Task<MarketData> GetMarketDataWithCacheAsync(StockCode stockCode)
    {
        const string cacheKeyPrefix = "marketdata";
        const int cacheDurationSeconds = 10;
        
        var cacheKey = $"{cacheKeyPrefix}:{stockCode}";
        
        // Redis 캐시 확인
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<MarketData>(cachedData);
        }
        
        // DB 조회
        var marketData = await _repository.GetByStockCodeAsync(stockCode);
        
        if (marketData != null)
        {
            // Redis에 저장
            var json = JsonSerializer.Serialize(marketData);
            await _cache.SetStringAsync(
                cacheKey,
                json,
                TimeSpan.FromSeconds(cacheDurationSeconds));
        }
        
        return marketData;
    }
}
```

### 10.2 데이터 압축

```csharp
/// <summary>
/// 시계열 데이터 압축 서비스
/// </summary>
public sealed class TimeSeriesCompressionService
{
    /// <summary>
    /// 오래된 캔들 데이터 압축
    /// 1분 캔들 → 5분 캔들로 병합
    /// </summary>
    public void CompressMinuteCandles(List<Candle> candles)
    {
        if (candles.Count < 5)
            return;
        
        var compressed = new List<Candle>();
        
        for (int i = 0; i < candles.Count; i += 5)
        {
            var batch = candles.Skip(i).Take(5).ToList();
            if (batch.Count < 5) break;
            
            var mergedCandle = Candle.Create(
                batch[0].Timestamp,
                batch[0].Open,
                batch.Max(c => c.High),
                batch.Min(c => c.Low),
                batch[4].Close,
                new TradingVolume(batch.Sum(c => c.Volume.Value)));
            
            compressed.Add(mergedCandle);
        }
    }
}
```

---

## 11. Security Considerations

### 11.1 데이터 검증

```csharp
/// <summary>
/// 시장 데이터 보안 검증 서비스
/// </summary>
public sealed class MarketDataSecurityService
{
    /// <summary>
    /// 시세 데이터 검증
    /// - 상한가/하한가 범위 확인
    /// - 이상치 탐지
    /// </summary>
    public bool ValidateMarketData(
        MarketData data,
        MarketData previousData,
        ValidationPolicy policy)
    {
        // 가격 급등 확인 (30% 이상 변동)
        if (previousData != null)
        {
            var changeRate = Math.Abs((data.CurrentPrice.Value - previousData.CurrentPrice.Value) / previousData.CurrentPrice.Value);
            
            if (changeRate > policy.MaxPriceChangeRate)
                return false;
        }
        
        // 거래량 이상치 확인
        if (previousData != null)
        {
            var volumeChange = (decimal)data.Volume.Value / previousData.Volume.Value;
            
            if (volumeChange > policy.MaxVolumeChangeRate)
                return false;
        }
        
        return true;
    }
}

/// <summary>
/// 검증 정책
/// </summary>
public sealed class ValidationPolicy
{
    public decimal MaxPriceChangeRate { get; } = 0.3m;  // 30%
    public decimal MaxVolumeChangeRate { get; } = 10m;  // 1000%
}
```

### 11.2 감사 로깅

```csharp
/// <summary>
/// 시장 데이터 감사 로깅
/// </summary>
public sealed class MarketDataAuditLogger
{
    private readonly ILogger<MarketDataAuditLogger> _auditLogger;
    
    /// <summary>
    /// 이상 시세 로깅
    /// </summary>
    public void LogAnomalousPrice(
        StockCode stockCode,
        MarketPrice price,
        PriceChange change)
    {
        _auditLogger.LogWarning(
            "AUDIT: Anomalous price detected - Stock: {StockCode}, Price: {Price}, Change: {Change}%",
            stockCode.Value,
            price.Value,
            change.ChangeRate);
    }
}
```

---

## 결론

이 Market Data Context 설계는 다음 특징을 갖습니다:

✅ **완전성**: 시장 데이터 관리의 모든 측면을 다룸
✅ **실시간 성능**: 밀리초 단위 응답 시간
✅ **데이터 정확성**: 엄격한 검증 및 이상치 탐지
✅ **확장성**: 수천 종목의 동시 처리
✅ **신뢰성**: 데이터 손실 없는 수신
✅ **분석**: 풍부한 기술적 지표 및 패턴 인식

이 설계를 기반으로 정확하고 빠른 시장 데이터 시스템을 구축할 수 있습니다.
