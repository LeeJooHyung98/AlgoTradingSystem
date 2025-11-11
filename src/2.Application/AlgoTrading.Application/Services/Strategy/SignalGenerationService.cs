using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// description($�) : Signal Generation Service Implementation (�� �1 D� l)
/// Details(�8$�) : Service for generating trading signals from strategies (�<\�0 p� �� �1 D�)
/// Applied technology patterns(�0 (4) : Service Pattern, Strategy Pattern, DDD Service (D� (4, � (4, DDD D�)
/// </summary>
public class SignalGenerationService : ISignalGenerationService
{
    private readonly ILogger<SignalGenerationService> _logger;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IPriceDataRepository _priceDataRepository;

    public SignalGenerationService(
        ILogger<SignalGenerationService> logger,
        IStrategyRepository strategyRepository,
        IPriceDataRepository priceDataRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategyRepository = strategyRepository ?? throw new ArgumentNullException(nameof(strategyRepository));
        _priceDataRepository = priceDataRepository ?? throw new ArgumentNullException(nameof(priceDataRepository));
    }

    public async Task<ErrorOr<TradingSignal>> GenerateSignalAsync(
        Guid strategyId,
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating signal for strategy {StrategyId} and stock {StockCode}",
            strategyId,
            stockCode);

        // 1. Load strategy from repository
        var strategy = await _strategyRepository.GetWithRulesAsync(strategyId, cancellationToken);
        if (strategy == null)
        {
            _logger.LogWarning("Strategy {StrategyId} not found", strategyId);
            return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
        }

        // Check if strategy is active
        if (!strategy.CanTradeNow())
        {
            _logger.LogDebug("Strategy {StrategyId} cannot trade now (inactive or outside trading hours)", strategyId);
            return new TradingSignal
            {
                SignalId = Guid.NewGuid(),
                StrategyId = strategyId,
                StockCode = stockCode,
                Type = SignalType.Hold,
                Strength = SignalStrength.Weak,
                GeneratedAt = DateTime.UtcNow,
                Indicators = new Dictionary<string, object>()
            };
        }

        // Check if strategy targets this stock
        var stockCodeValue = new Core.ValueObjects.StockCode(stockCode);
        if (!strategy.TargetsStock(stockCodeValue))
        {
            _logger.LogDebug("Strategy {StrategyId} does not target stock {StockCode}", strategyId, stockCode);
            return new TradingSignal
            {
                SignalId = Guid.NewGuid(),
                StrategyId = strategyId,
                StockCode = stockCode,
                Type = SignalType.Hold,
                Strength = SignalStrength.Weak,
                GeneratedAt = DateTime.UtcNow,
                Indicators = new Dictionary<string, object>()
            };
        }

        // 2. Load latest market data for the stock (last 60 days for technical indicators)
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-60);

        var priceData = await _priceDataRepository.GetByStockCodeAndDateRangeAsync(
            stockCode,
            startDate,
            endDate,
            cancellationToken);

        var priceDataList = priceData.OrderBy(p => p.Timestamp).ToList();

        if (!priceDataList.Any())
        {
            _logger.LogWarning("No price data found for stock {StockCode}", stockCode);
            return Error.NotFound("PriceData.NotFound", $"No price data found for stock {stockCode}");
        }

        // 3. Calculate technical indicators
        var indicators = CalculateTechnicalIndicators(priceDataList);

        // 4. Apply strategy rules and generate signal
        var signal = GenerateSignalFromIndicators(strategy, stockCode, priceDataList, indicators);

        _logger.LogInformation(
            "Generated signal for {StockCode}: Type={Type}, Strength={Strength}",
            stockCode,
            signal.Type,
            signal.Strength);

        return signal;
    }

    public async Task<ErrorOr<IEnumerable<TradingSignal>>> GenerateSignalsAsync(
        Guid strategyId,
        IEnumerable<string> stockCodes,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating signals for strategy {StrategyId} and {Count} stocks",
            strategyId,
            stockCodes.Count());

        var signals = new List<TradingSignal>();

        foreach (var stockCode in stockCodes)
        {
            var signalResult = await GenerateSignalAsync(strategyId, stockCode, cancellationToken);

            if (signalResult.IsError)
            {
                _logger.LogWarning(
                    "Failed to generate signal for {StockCode}",
                    stockCode);
                continue;
            }

            signals.Add(signalResult.Value);
        }

        return signals;
    }

    public async Task<ErrorOr<bool>> EvaluateEntryConditionsAsync(
        Guid strategyId,
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Evaluating entry conditions for strategy {StrategyId} and stock {StockCode}",
            strategyId,
            stockCode);

        // 1. Load strategy entry rules
        var strategy = await _strategyRepository.GetWithRulesAsync(strategyId, cancellationToken);
        if (strategy == null)
        {
            return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
        }

        // Check if strategy can trade now
        if (!strategy.CanTradeNow())
        {
            _logger.LogDebug("Strategy {StrategyId} cannot trade now", strategyId);
            return false;
        }

        // Get entry rules
        var entryRules = strategy.Rules
            .Where(r => r.IsEnabled && r.Type == Core.Entities.Strategy.RuleType.EntryLong)
            .OrderByDescending(r => r.Priority)
            .ToList();

        if (!entryRules.Any())
        {
            _logger.LogDebug("Strategy {StrategyId} has no enabled entry rules", strategyId);
            return false;
        }

        // 2. Load latest market data
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-60);

        var priceData = await _priceDataRepository.GetByStockCodeAndDateRangeAsync(
            stockCode,
            startDate,
            endDate,
            cancellationToken);

        var priceDataList = priceData.OrderBy(p => p.Timestamp).ToList();

        if (!priceDataList.Any())
        {
            _logger.LogWarning("No price data found for stock {StockCode}", stockCode);
            return false;
        }

        // 3. Evaluate all entry conditions
        var indicators = CalculateTechnicalIndicators(priceDataList);

        // Simplified evaluation: Check if price crosses above SMA20
        // TODO: Implement proper rule evaluation engine
        var currentPrice = priceDataList.Last().CurrentPrice;
        var sma20 = indicators.ContainsKey("SMA20") ? (decimal)indicators["SMA20"] : 0;

        var entryConditionMet = currentPrice > sma20;

        _logger.LogDebug(
            "Entry condition evaluation for {StockCode}: Price={Price}, SMA20={SMA20}, Met={Met}",
            stockCode,
            currentPrice,
            sma20,
            entryConditionMet);

        // 4. Return true if all conditions are met
        return entryConditionMet;
    }

    public async Task<ErrorOr<bool>> EvaluateExitConditionsAsync(
        Guid strategyId,
        string stockCode,
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Evaluating exit conditions for strategy {StrategyId}, stock {StockCode}, position {PositionId}",
            strategyId,
            stockCode,
            positionId);

        // 1. Load strategy exit rules
        var strategy = await _strategyRepository.GetWithRulesAsync(strategyId, cancellationToken);
        if (strategy == null)
        {
            return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
        }

        // Get exit rules
        var exitRules = strategy.Rules
            .Where(r => r.IsEnabled && (
                r.Type == Core.Entities.Strategy.RuleType.ExitLong ||
                r.Type == Core.Entities.Strategy.RuleType.StopLoss ||
                r.Type == Core.Entities.Strategy.RuleType.TakeProfit))
            .OrderByDescending(r => r.Priority)
            .ToList();

        // 2. Load latest market data
        var latestPrice = await _priceDataRepository.GetLatestByStockCodeAsync(stockCode, cancellationToken);
        if (latestPrice == null)
        {
            _logger.LogWarning("No price data found for stock {StockCode}", stockCode);
            return false;
        }

        // 3. Load historical data for indicator calculation
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-60);

        var priceData = await _priceDataRepository.GetByStockCodeAndDateRangeAsync(
            stockCode,
            startDate,
            endDate,
            cancellationToken);

        var priceDataList = priceData.OrderBy(p => p.Timestamp).ToList();

        if (!priceDataList.Any())
        {
            return false;
        }

        // 4. Evaluate exit conditions
        var indicators = CalculateTechnicalIndicators(priceDataList);

        // Simplified evaluation: Check if price crosses below SMA20
        // TODO: Implement proper rule evaluation engine with position P&L check
        var currentPrice = latestPrice.CurrentPrice;
        var sma20 = indicators.ContainsKey("SMA20") ? (decimal)indicators["SMA20"] : 0;

        var exitConditionMet = currentPrice < sma20;

        _logger.LogDebug(
            "Exit condition evaluation for {StockCode}: Price={Price}, SMA20={SMA20}, Met={Met}",
            stockCode,
            currentPrice,
            sma20,
            exitConditionMet);

        // 5. Return true if any exit condition is met
        return exitConditionMet;
    }

    /// <summary>
    /// Calculate technical indicators from price data (가격 데이터에서 기술적 지표 계산)
    /// </summary>
    private Dictionary<string, object> CalculateTechnicalIndicators(List<Core.Entities.MarketData.PriceData> priceDataList)
    {
        var indicators = new Dictionary<string, object>();

        if (priceDataList.Count < 20)
        {
            return indicators; // Not enough data for indicators
        }

        // Simple Moving Average (SMA) - 20 period
        var sma20 = priceDataList.TakeLast(20).Average(p => p.CurrentPrice);
        indicators["SMA20"] = sma20;

        // Simple Moving Average (SMA) - 50 period
        if (priceDataList.Count >= 50)
        {
            var sma50 = priceDataList.TakeLast(50).Average(p => p.CurrentPrice);
            indicators["SMA50"] = sma50;
        }

        // Relative Strength Index (RSI) - 14 period (simplified)
        if (priceDataList.Count >= 14)
        {
            var rsi = CalculateRSI(priceDataList, 14);
            indicators["RSI"] = rsi;
        }

        // MACD (Moving Average Convergence Divergence) - simplified
        if (priceDataList.Count >= 26)
        {
            var ema12 = CalculateEMA(priceDataList, 12);
            var ema26 = CalculateEMA(priceDataList, 26);
            var macd = ema12 - ema26;
            indicators["MACD"] = macd;
            indicators["EMA12"] = ema12;
            indicators["EMA26"] = ema26;
        }

        // Volume indicators
        var avgVolume20 = priceDataList.TakeLast(20).Average(p => (decimal)p.Volume);
        indicators["AvgVolume20"] = avgVolume20;

        // Current price
        var currentPrice = priceDataList.Last().CurrentPrice;
        indicators["CurrentPrice"] = currentPrice;

        return indicators;
    }

    /// <summary>
    /// Calculate RSI (Relative Strength Index) (상대강도지수 계산)
    /// </summary>
    private decimal CalculateRSI(List<Core.Entities.MarketData.PriceData> priceDataList, int period)
    {
        var prices = priceDataList.TakeLast(period + 1).Select(p => p.CurrentPrice).ToList();

        if (prices.Count < period + 1)
        {
            return 50; // Neutral RSI if not enough data
        }

        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = 1; i < prices.Count; i++)
        {
            var change = prices[i] - prices[i - 1];
            if (change > 0)
            {
                gains.Add(change);
                losses.Add(0);
            }
            else
            {
                gains.Add(0);
                losses.Add(Math.Abs(change));
            }
        }

        var avgGain = gains.Average();
        var avgLoss = losses.Average();

        if (avgLoss == 0)
        {
            return 100; // All gains, RSI = 100
        }

        var rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        return rsi;
    }

    /// <summary>
    /// Calculate EMA (Exponential Moving Average) (지수이동평균 계산)
    /// </summary>
    private decimal CalculateEMA(List<Core.Entities.MarketData.PriceData> priceDataList, int period)
    {
        var prices = priceDataList.TakeLast(period).Select(p => p.CurrentPrice).ToList();

        if (prices.Count < period)
        {
            return prices.Average(); // Fall back to SMA if not enough data
        }

        var multiplier = 2m / (period + 1);
        var ema = prices.First(); // Start with first price as initial EMA

        for (int i = 1; i < prices.Count; i++)
        {
            ema = (prices[i] - ema) * multiplier + ema;
        }

        return ema;
    }

    /// <summary>
    /// Generate trading signal from indicators and strategy rules (지표와 전략 규칙에서 거래 신호 생성)
    /// </summary>
    private TradingSignal GenerateSignalFromIndicators(
        Core.Entities.Strategy.TradingStrategy strategy,
        string stockCode,
        List<Core.Entities.MarketData.PriceData> priceDataList,
        Dictionary<string, object> indicators)
    {
        var currentPrice = priceDataList.Last().CurrentPrice;
        var signalType = SignalType.Hold;
        var signalStrength = SignalStrength.Weak;

        // Simplified signal generation based on multiple indicators
        // TODO: Replace with proper strategy rule evaluation engine

        if (indicators.ContainsKey("SMA20") && indicators.ContainsKey("RSI"))
        {
            var sma20 = (decimal)indicators["SMA20"];
            var rsi = (decimal)indicators["RSI"];

            // Buy signal conditions
            if (currentPrice > sma20 && rsi < 30) // Oversold and above SMA
            {
                signalType = SignalType.Buy;
                signalStrength = SignalStrength.Strong;
            }
            else if (currentPrice > sma20 && rsi < 50) // Above SMA, moderate RSI
            {
                signalType = SignalType.Buy;
                signalStrength = SignalStrength.Moderate;
            }
            else if (currentPrice > sma20) // Just above SMA
            {
                signalType = SignalType.Buy;
                signalStrength = SignalStrength.Weak;
            }
            // Sell signal conditions
            else if (currentPrice < sma20 && rsi > 70) // Overbought and below SMA
            {
                signalType = SignalType.Sell;
                signalStrength = SignalStrength.Strong;
            }
            else if (currentPrice < sma20 && rsi > 50) // Below SMA, moderate RSI
            {
                signalType = SignalType.Sell;
                signalStrength = SignalStrength.Moderate;
            }
            else if (currentPrice < sma20) // Just below SMA
            {
                signalType = SignalType.Sell;
                signalStrength = SignalStrength.Weak;
            }
        }

        // Calculate target price and stop loss based on strategy parameters
        decimal? targetPrice = null;
        decimal? stopLoss = null;

        if (signalType == SignalType.Buy)
        {
            targetPrice = currentPrice * (1 + strategy.TakeProfitPercent / 100);
            stopLoss = currentPrice * (1 - strategy.StopLossPercent / 100);
        }
        else if (signalType == SignalType.Sell)
        {
            targetPrice = currentPrice * (1 - strategy.TakeProfitPercent / 100);
            stopLoss = currentPrice * (1 + strategy.StopLossPercent / 100);
        }

        return new TradingSignal
        {
            SignalId = Guid.NewGuid(),
            StrategyId = strategy.Id.Value,
            StockCode = stockCode,
            Type = signalType,
            Strength = signalStrength,
            TargetPrice = targetPrice,
            StopLoss = stopLoss,
            TakeProfit = targetPrice,
            GeneratedAt = DateTime.UtcNow,
            Indicators = indicators
        };
    }
}
