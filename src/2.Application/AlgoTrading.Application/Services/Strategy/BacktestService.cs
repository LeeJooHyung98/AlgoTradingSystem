using AlgoTrading.Application.Services.MarketData;
using AlgoTrading.Application.Services.Strategy.Evaluation;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// description(설명) : Backtest Service Implementation (백테스트 서비스 구현)
/// Details(상세설명) : Service for executing backtests on trading strategies (트레이딩 전략의 백테스트 실행 서비스)
/// Applied technology patterns(적용기술패턴) : Service Pattern, Strategy Pattern, DDD Service (서비스 패턴, 전략 패턴, DDD 서비스)
/// </summary>
public class BacktestService : IBacktestService
{
    private readonly ILogger<BacktestService> _logger;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IPriceDataRepository _priceDataRepository;
    private readonly RuleEvaluationEngine _ruleEvaluationEngine;

    public BacktestService(
        ILogger<BacktestService> logger,
        IStrategyRepository strategyRepository,
        IPriceDataRepository priceDataRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategyRepository = strategyRepository ?? throw new ArgumentNullException(nameof(strategyRepository));
        _priceDataRepository = priceDataRepository ?? throw new ArgumentNullException(nameof(priceDataRepository));
        _ruleEvaluationEngine = new RuleEvaluationEngine(_logger);
    }

    public async Task<ErrorOr<BacktestResult>> RunBacktestAsync(
        Guid strategyId,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Running backtest for strategy {StrategyId} from {StartDate} to {EndDate} with capital {InitialCapital}",
            strategyId,
            startDate,
            endDate,
            initialCapital);

        // 1. Load strategy from repository
        var strategy = await _strategyRepository.GetWithRulesAsync(strategyId, cancellationToken);
        if (strategy == null)
        {
            _logger.LogWarning("Strategy {StrategyId} not found", strategyId);
            return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
        }

        // 2. Validate backtest parameters
        if (startDate >= endDate)
        {
            return Error.Validation("Backtest.InvalidDateRange", "Start date must be before end date");
        }

        if (initialCapital <= 0)
        {
            return Error.Validation("Backtest.InvalidCapital", "Initial capital must be greater than zero");
        }

        // 3. Initialize backtest state
        var backtestState = new BacktestState
        {
            CurrentCapital = initialCapital,
            InitialCapital = initialCapital,
            Trades = new List<BacktestTrade>(),
            EquityCurve = new List<EquityCurvePoint>(),
            OpenPositions = new Dictionary<string, BacktestPosition>()
        };

        // 4. Load historical price data for strategy's target stocks
        var targetStocks = strategy.TargetStocks.Any()
            ? strategy.TargetStocks.Select(s => s.Value).ToList()
            : new List<string>(); // Empty list means no specific target stocks defined

        if (!targetStocks.Any())
        {
            _logger.LogWarning("Strategy {StrategyId} has no target stocks defined, backtest will have no trades", strategyId);
        }
        else
        {
            // Execute backtest for each target stock
            foreach (var stockCode in targetStocks)
            {
                _logger.LogDebug("Loading price data for stock {StockCode} from {StartDate} to {EndDate}",
                    stockCode, startDate, endDate);

                var priceData = await _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                    stockCode,
                    startDate,
                    endDate,
                    cancellationToken);

                var priceList = priceData.OrderBy(p => p.Timestamp).ToList();

                if (!priceList.Any())
                {
                    _logger.LogWarning("No price data found for stock {StockCode} in date range", stockCode);
                    continue;
                }

                // Execute strategy on this stock's price data
                await ExecuteBacktestForStock(
                    strategy,
                    stockCode,
                    priceList,
                    backtestState,
                    cancellationToken);
            }
        }

        _logger.LogInformation("Backtest simulation completed for strategy {StrategyId}", strategyId);

        // 5. Calculate performance metrics
        var metrics = CalculatePerformanceMetrics(backtestState.Trades, initialCapital);

        // 5. Build result
        var result = new BacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = startDate,
            EndDate = endDate,
            InitialCapital = initialCapital,
            FinalCapital = backtestState.CurrentCapital,
            Metrics = metrics,
            Trades = backtestState.Trades,
            EquityCurve = backtestState.EquityCurve
        };

        _logger.LogInformation(
            "Backtest completed. Initial: {Initial}, Final: {Final}, Total Trades: {Trades}, Win Rate: {WinRate:P2}",
            initialCapital,
            result.FinalCapital,
            result.Trades.Count,
            metrics.WinRate);

        return result;
    }

    public async Task<ErrorOr<WalkForwardBacktestResult>> RunWalkForwardBacktestAsync(
        Guid strategyId,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        int inSamplePeriodDays,
        int outOfSamplePeriodDays,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Running walk-forward backtest for strategy {StrategyId} with in-sample: {InSample} days, out-of-sample: {OutOfSample} days",
            strategyId,
            inSamplePeriodDays,
            outOfSamplePeriodDays);

        // Validate parameters
        if (inSamplePeriodDays <= 0 || outOfSamplePeriodDays <= 0)
        {
            return Error.Validation(
                "Backtest.InvalidPeriods",
                "In-sample and out-of-sample periods must be greater than zero");
        }

        var strategy = await _strategyRepository.GetWithRulesAsync(strategyId, cancellationToken);
        if (strategy == null)
        {
            return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
        }

        // Split backtest period into walk-forward windows
        var periodResults = new List<WalkForwardPeriodResult>();
        var currentDate = startDate;
        var windowPeriodDays = inSamplePeriodDays + outOfSamplePeriodDays;

        while (currentDate.AddDays(windowPeriodDays) <= endDate)
        {
            var inSampleStart = currentDate;
            var inSampleEnd = currentDate.AddDays(inSamplePeriodDays);
            var outOfSampleStart = inSampleEnd;
            var outOfSampleEnd = inSampleEnd.AddDays(outOfSamplePeriodDays);

            _logger.LogDebug(
                "Processing walk-forward period: In-Sample {InStart} to {InEnd}, Out-Of-Sample {OutStart} to {OutEnd}",
                inSampleStart,
                inSampleEnd,
                outOfSampleStart,
                outOfSampleEnd);

            // Run in-sample optimization (for now, just run backtest)
            var inSampleResult = await RunBacktestAsync(
                strategyId,
                inSampleStart,
                inSampleEnd,
                initialCapital,
                cancellationToken);

            if (inSampleResult.IsError)
            {
                _logger.LogWarning("In-sample backtest failed for period {Start} to {End}", inSampleStart, inSampleEnd);
                currentDate = currentDate.AddDays(outOfSamplePeriodDays);
                continue;
            }

            // Run out-of-sample test
            var outOfSampleResult = await RunBacktestAsync(
                strategyId,
                outOfSampleStart,
                outOfSampleEnd,
                initialCapital,
                cancellationToken);

            if (outOfSampleResult.IsError)
            {
                _logger.LogWarning("Out-of-sample backtest failed for period {Start} to {End}", outOfSampleStart, outOfSampleEnd);
                currentDate = currentDate.AddDays(outOfSamplePeriodDays);
                continue;
            }

            periodResults.Add(new WalkForwardPeriodResult
            {
                InSampleStart = inSampleStart,
                InSampleEnd = inSampleEnd,
                OutOfSampleStart = outOfSampleStart,
                OutOfSampleEnd = outOfSampleEnd,
                InSampleMetrics = inSampleResult.Value.Metrics,
                OutOfSampleMetrics = outOfSampleResult.Value.Metrics
            });

            currentDate = outOfSampleEnd;
        }

        // Calculate overall metrics
        var allTrades = new List<BacktestTrade>();
        foreach (var period in periodResults)
        {
            // In practice, we'd collect trades from out-of-sample periods only
        }

        var overallMetrics = CalculatePerformanceMetrics(allTrades, initialCapital);

        var result = new WalkForwardBacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = startDate,
            EndDate = endDate,
            InitialCapital = initialCapital,
            FinalCapital = initialCapital, // TODO: Calculate from out-of-sample trades
            OverallMetrics = overallMetrics,
            PeriodResults = periodResults
        };

        _logger.LogInformation(
            "Walk-forward backtest completed with {Periods} periods",
            periodResults.Count);

        return result;
    }

    /// <summary>
    /// Execute backtest simulation for a single stock (단일 종목에 대한 백테스트 시뮬레이션 실행)
    /// </summary>
    private Task ExecuteBacktestForStock(
        Core.Entities.Strategy.TradingStrategy strategy,
        string stockCode,
        List<Core.Entities.MarketData.PriceData> priceDataList,
        BacktestState state,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing backtest for stock {StockCode} with {DataPoints} price data points",
            stockCode,
            priceDataList.Count);

        // Calculate position size based on max position size percent
        var maxPositionValue = state.InitialCapital * (strategy.MaxPositionSizePercent / 100);

        for (int i = 0; i < priceDataList.Count; i++)
        {
            var currentPrice = priceDataList[i];
            var currentTime = currentPrice.Timestamp;

            // Update equity curve
            var currentEquity = CalculateCurrentEquity(state, priceDataList, i);
            var peak = state.EquityCurve.Any() ? state.EquityCurve.Max(e => e.Equity) : state.InitialCapital;
            if (currentEquity > peak) peak = currentEquity;
            var drawdown = peak - currentEquity;
            var drawdownPercent = peak > 0 ? drawdown / peak : 0;

            state.EquityCurve.Add(new EquityCurvePoint
            {
                Time = currentTime,
                Equity = currentEquity,
                Drawdown = drawdown,
                DrawdownPercent = drawdownPercent
            });

            // Check exit conditions for open positions
            if (state.OpenPositions.TryGetValue(stockCode, out var position))
            {
                var currentPriceValue = currentPrice.CurrentPrice;
                var entryPrice = position.EntryPrice;
                var priceChange = (currentPriceValue - entryPrice) / entryPrice;
                var priceChangePercent = priceChange * 100;

                // Check stop loss
                if (priceChangePercent <= -strategy.StopLossPercent)
                {
                    ClosePosition(state, position, currentPriceValue, currentTime, "Stop Loss");
                    continue;
                }

                // Check take profit
                if (priceChangePercent >= strategy.TakeProfitPercent)
                {
                    ClosePosition(state, position, currentPriceValue, currentTime, "Take Profit");
                    continue;
                }

                // Check strategy exit rules
                if (i >= 20)
                {
                    var historicalData = ConvertToHistoricalPriceData(priceDataList.Take(i + 1).ToList(), stockCode);
                    var currentHistoricalPrice = historicalData.Last();

                    var exitRules = strategy.Rules
                        .Where(r => r.Type == Core.Entities.Strategy.RuleType.ExitLong)
                        .ToList();

                    foreach (var rule in exitRules)
                    {
                        if (_ruleEvaluationEngine.EvaluateRule(rule, historicalData, currentHistoricalPrice))
                        {
                            _logger.LogDebug("Exit rule '{RuleName}' triggered for {StockCode}", rule.Name, stockCode);
                            ClosePosition(state, position, currentPriceValue, currentTime, $"Exit Rule: {rule.Name}");
                            break;
                        }
                    }
                }
            }
            else
            {
                // Check if we should enter a new position using strategy rules
                if (i >= 20 && state.CurrentCapital > 0)
                {
                    // Convert PriceData to HistoricalPriceData
                    var historicalData = ConvertToHistoricalPriceData(priceDataList.Take(i + 1).ToList(), stockCode);
                    var currentHistoricalPrice = historicalData.Last();

                    // Evaluate entry rules
                    var entryRules = strategy.Rules
                        .Where(r => r.Type == Core.Entities.Strategy.RuleType.EntryLong)
                        .ToList();

                    bool shouldEnter = false;
                    foreach (var rule in entryRules)
                    {
                        if (_ruleEvaluationEngine.EvaluateRule(rule, historicalData, currentHistoricalPrice))
                        {
                            shouldEnter = true;
                            _logger.LogDebug("Entry rule '{RuleName}' triggered for {StockCode}", rule.Name, stockCode);
                            break;
                        }
                    }

                    if (shouldEnter)
                    {
                        var currentPriceValue = currentPrice.CurrentPrice;

                        // Calculate position size
                        var positionValue = Math.Min(maxPositionValue, state.CurrentCapital);
                        var quantity = (int)(positionValue / currentPriceValue);

                        if (quantity > 0)
                        {
                            var actualPositionValue = quantity * currentPriceValue;

                            // Open position
                            state.OpenPositions[stockCode] = new BacktestPosition
                            {
                                StockCode = stockCode,
                                EntryTime = currentTime,
                                EntryPrice = currentPriceValue,
                                Quantity = quantity
                            };

                            state.CurrentCapital -= actualPositionValue;

                            _logger.LogTrace(
                                "Opened position: {StockCode} @ {Price}, Quantity: {Quantity}, Capital remaining: {Capital}",
                                stockCode,
                                currentPriceValue,
                                quantity,
                                state.CurrentCapital);
                        }
                    }
                }
            }
        }

        // Close any remaining open positions at the end of backtest
        if (state.OpenPositions.TryGetValue(stockCode, out var finalPosition))
        {
            var lastPrice = priceDataList.Last();
            ClosePosition(state, finalPosition, lastPrice.CurrentPrice, lastPrice.Timestamp, "End of Backtest");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Calculate current equity (cash + open positions value) (현재 자산 계산: 현금 + 열린 포지션 가치)
    /// </summary>
    private decimal CalculateCurrentEquity(
        BacktestState state,
        List<Core.Entities.MarketData.PriceData> priceDataList,
        int currentIndex)
    {
        var equity = state.CurrentCapital;

        foreach (var position in state.OpenPositions.Values)
        {
            var currentPrice = priceDataList[currentIndex].CurrentPrice;
            equity += position.Quantity * currentPrice;
        }

        return equity;
    }

    /// <summary>
    /// Close an open position and record the trade (열린 포지션을 청산하고 거래 기록)
    /// </summary>
    private void ClosePosition(
        BacktestState state,
        BacktestPosition position,
        decimal exitPrice,
        DateTime exitTime,
        string exitReason)
    {
        var exitValue = position.Quantity * exitPrice;
        var entryValue = position.Quantity * position.EntryPrice;
        var realizedPL = exitValue - entryValue;
        var realizedPLPercent = (exitPrice - position.EntryPrice) / position.EntryPrice;

        // Record trade
        state.Trades.Add(new BacktestTrade
        {
            StockCode = position.StockCode,
            EntryTime = position.EntryTime,
            ExitTime = exitTime,
            EntryPrice = position.EntryPrice,
            ExitPrice = exitPrice,
            Quantity = position.Quantity,
            RealizedPL = realizedPL,
            RealizedPLPercent = realizedPLPercent
        });

        // Return capital
        state.CurrentCapital += exitValue;

        // Remove position
        state.OpenPositions.Remove(position.StockCode);

        _logger.LogTrace(
            "Closed position: {StockCode} @ {ExitPrice}, P&L: {PL:F2} ({PLPercent:P2}), Reason: {Reason}",
            position.StockCode,
            exitPrice,
            realizedPL,
            realizedPLPercent,
            exitReason);
    }

    public PerformanceMetrics CalculatePerformanceMetrics(
        IEnumerable<BacktestTrade> trades,
        decimal initialCapital)
    {
        var tradesList = trades.ToList();

        if (!tradesList.Any())
        {
            return new PerformanceMetrics
            {
                TotalReturn = 0,
                AnnualizedReturn = 0,
                SharpeRatio = 0,
                MaxDrawdown = 0,
                MaxDrawdownPercent = 0,
                TotalTrades = 0,
                WinningTrades = 0,
                LosingTrades = 0,
                WinRate = 0,
                ProfitFactor = 0,
                AverageWin = 0,
                AverageLoss = 0,
                LargestWin = 0,
                LargestLoss = 0
            };
        }

        // Calculate basic trade statistics
        var totalPL = tradesList.Sum(t => t.RealizedPL);
        var finalCapital = initialCapital + totalPL;
        var totalReturn = totalPL / initialCapital;

        var winningTrades = tradesList.Where(t => t.RealizedPL > 0).ToList();
        var losingTrades = tradesList.Where(t => t.RealizedPL < 0).ToList();

        var totalWin = winningTrades.Sum(t => t.RealizedPL);
        var totalLoss = Math.Abs(losingTrades.Sum(t => t.RealizedPL));

        // Calculate time-based metrics
        var firstTradeDate = tradesList.Min(t => t.EntryTime);
        var lastTradeDate = tradesList.Max(t => t.ExitTime);
        var totalDays = (lastTradeDate - firstTradeDate).TotalDays;
        var annualizedReturn = totalDays > 0 ? totalReturn * (decimal)(365.0 / totalDays) : 0;

        // Calculate Sharpe Ratio (simplified)
        var returns = tradesList.Select(t => (double)t.RealizedPLPercent).ToList();
        var avgReturn = returns.Any() ? returns.Average() : 0;
        var stdDev = returns.Any() && returns.Count > 1
            ? Math.Sqrt(returns.Select(r => Math.Pow(r - avgReturn, 2)).Sum() / (returns.Count - 1))
            : 0;
        var sharpeRatio = stdDev > 0 ? (decimal)(avgReturn / stdDev * Math.Sqrt(252)) : 0; // Assuming daily returns

        // Calculate Max Drawdown
        var equity = initialCapital;
        var peak = initialCapital;
        var maxDrawdown = 0m;
        var maxDrawdownPercent = 0m;

        foreach (var trade in tradesList.OrderBy(t => t.ExitTime))
        {
            equity += trade.RealizedPL;
            if (equity > peak)
            {
                peak = equity;
            }
            var drawdown = peak - equity;
            var drawdownPercent = peak > 0 ? drawdown / peak : 0;

            if (drawdown > maxDrawdown)
            {
                maxDrawdown = drawdown;
                maxDrawdownPercent = drawdownPercent;
            }
        }

        var metrics = new PerformanceMetrics
        {
            TotalReturn = totalReturn,
            AnnualizedReturn = annualizedReturn,
            SharpeRatio = sharpeRatio,
            MaxDrawdown = maxDrawdown,
            MaxDrawdownPercent = maxDrawdownPercent,
            TotalTrades = tradesList.Count,
            WinningTrades = winningTrades.Count,
            LosingTrades = losingTrades.Count,
            WinRate = tradesList.Count > 0 ? (decimal)winningTrades.Count / tradesList.Count : 0,
            ProfitFactor = totalLoss > 0 ? totalWin / totalLoss : 0,
            AverageWin = winningTrades.Any() ? winningTrades.Average(t => t.RealizedPL) : 0,
            AverageLoss = losingTrades.Any() ? losingTrades.Average(t => t.RealizedPL) : 0,
            LargestWin = winningTrades.Any() ? winningTrades.Max(t => t.RealizedPL) : 0,
            LargestLoss = losingTrades.Any() ? losingTrades.Min(t => t.RealizedPL) : 0
        };

        return metrics;
    }

    /// <summary>
    /// Convert PriceData entities to HistoricalPriceData for rule evaluation
    /// </summary>
    private List<HistoricalPriceData> ConvertToHistoricalPriceData(
        List<Core.Entities.MarketData.PriceData> priceDataList,
        string stockCode)
    {
        return priceDataList.Select(p => new HistoricalPriceData
        {
            StockCode = stockCode,
            Date = p.Timestamp,
            Open = p.OpenPrice,
            High = p.HighPrice,
            Low = p.LowPrice,
            Close = p.CurrentPrice,
            Volume = (long)p.Volume,
            AdjustedClose = p.CurrentPrice
        }).ToList();
    }

    /// <summary>
    /// Internal backtest state (내부 백테스트 상태)
    /// </summary>
    private class BacktestState
    {
        public decimal CurrentCapital { get; set; }
        public decimal InitialCapital { get; set; }
        public List<BacktestTrade> Trades { get; set; } = new();
        public List<EquityCurvePoint> EquityCurve { get; set; } = new();
        public Dictionary<string, BacktestPosition> OpenPositions { get; set; } = new();
    }

    /// <summary>
    /// Internal backtest position (내부 백테스트 포지션)
    /// </summary>
    private class BacktestPosition
    {
        public string StockCode { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public decimal EntryPrice { get; set; }
        public int Quantity { get; set; }
    }
}
