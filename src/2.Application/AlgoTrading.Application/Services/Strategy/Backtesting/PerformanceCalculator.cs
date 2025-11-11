using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Application.Services.Strategy.Backtesting;

/// <summary>
/// description(설명) : Performance Calculator for Backtesting (백테스팅용 성과 계산기)
/// Details(상세설명) : Calculates comprehensive performance metrics from backtest results
/// Applied technology patterns(적용기술패턴) : Financial Calculations, Statistical Analysis
/// </summary>
public class PerformanceCalculator
{
    private const decimal RiskFreeRate = 0.02m; // 2% annual risk-free rate
    private const int TradingDaysPerYear = 252;

    /// <summary>
    /// Calculate all performance metrics from backtest results
    /// </summary>
    public static PerformanceMetrics Calculate(
        IEnumerable<BacktestTrade> trades,
        decimal initialCapital,
        decimal finalCapital,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<EquityCurvePoint>? equityCurve = null)
    {
        var tradesList = trades.ToList();

        // Basic trade statistics
        var totalTrades = tradesList.Count;
        var winningTrades = tradesList.Count(t => t.RealizedPL > 0);
        var losingTrades = tradesList.Count(t => t.RealizedPL < 0);
        var breakEvenTrades = tradesList.Count(t => t.RealizedPL == 0);

        // Calculate returns
        var totalReturn = initialCapital > 0
            ? ((finalCapital - initialCapital) / initialCapital) * 100
            : 0m;

        var tradingDays = (endDate - startDate).Days;
        var years = tradingDays / 365.25m;
        var annualizedReturn = years > 0
            ? ((decimal)Math.Pow((double)(finalCapital / initialCapital), (double)(1 / years)) - 1) * 100
            : 0m;

        // Win rate
        var winRate = totalTrades > 0
            ? (decimal)winningTrades / totalTrades * 100
            : 0m;

        // Profit/Loss calculations
        var wins = tradesList.Where(t => t.RealizedPL > 0).ToList();
        var losses = tradesList.Where(t => t.RealizedPL < 0).ToList();

        var totalWins = wins.Sum(t => t.RealizedPL);
        var totalLosses = Math.Abs(losses.Sum(t => t.RealizedPL));

        var averageWin = winningTrades > 0
            ? totalWins / winningTrades
            : 0m;

        var averageLoss = losingTrades > 0
            ? totalLosses / losingTrades
            : 0m;

        var largestWin = wins.Any()
            ? wins.Max(t => t.RealizedPL)
            : 0m;

        var largestLoss = losses.Any()
            ? losses.Min(t => t.RealizedPL)
            : 0m;

        // Profit factor
        var profitFactor = totalLosses > 0
            ? totalWins / totalLosses
            : (totalWins > 0 ? decimal.MaxValue : 0m);

        // Drawdown calculations
        var (maxDrawdown, maxDrawdownPercent) = CalculateMaxDrawdown(equityCurve, initialCapital);

        // Sharpe ratio
        var sharpeRatio = CalculateSharpeRatio(tradesList, tradingDays);

        return new PerformanceMetrics
        {
            TotalReturn = totalReturn,
            AnnualizedReturn = annualizedReturn,
            SharpeRatio = sharpeRatio,
            MaxDrawdown = maxDrawdown,
            MaxDrawdownPercent = maxDrawdownPercent,
            TotalTrades = totalTrades,
            WinningTrades = winningTrades,
            LosingTrades = losingTrades,
            WinRate = winRate,
            ProfitFactor = profitFactor,
            AverageWin = averageWin,
            AverageLoss = averageLoss,
            LargestWin = largestWin,
            LargestLoss = largestLoss
        };
    }

    /// <summary>
    /// Calculate Sharpe Ratio (risk-adjusted return)
    /// </summary>
    private static decimal CalculateSharpeRatio(List<BacktestTrade> trades, int tradingDays)
    {
        if (trades.Count < 2)
            return 0m;

        // Calculate returns per trade
        var returns = trades.Select(t => t.RealizedPLPercent / 100).ToList();

        // Calculate average return
        var avgReturn = returns.Average();

        // Calculate standard deviation of returns
        var variance = returns.Sum(r => (r - avgReturn) * (r - avgReturn)) / (returns.Count - 1);
        var stdDev = (decimal)Math.Sqrt((double)variance);

        if (stdDev == 0m)
            return 0m;

        // Annualize the Sharpe ratio
        var tradesPerYear = tradingDays > 0
            ? (decimal)trades.Count / tradingDays * TradingDaysPerYear
            : TradingDaysPerYear;

        var annualizedAvgReturn = avgReturn * tradesPerYear;
        var annualizedStdDev = stdDev * (decimal)Math.Sqrt((double)tradesPerYear);

        // Sharpe Ratio = (Annual Return - Risk Free Rate) / Annual Std Dev
        var sharpeRatio = annualizedStdDev > 0
            ? (annualizedAvgReturn - RiskFreeRate) / annualizedStdDev
            : 0m;

        return sharpeRatio;
    }

    /// <summary>
    /// Calculate maximum drawdown from equity curve
    /// </summary>
    private static (decimal maxDrawdown, decimal maxDrawdownPercent) CalculateMaxDrawdown(
        IEnumerable<EquityCurvePoint>? equityCurve,
        decimal initialCapital)
    {
        if (equityCurve == null || !equityCurve.Any())
            return (0m, 0m);

        var equityPoints = equityCurve.ToList();

        // Find maximum drawdown
        decimal peak = initialCapital;
        decimal maxDrawdown = 0m;
        decimal maxDrawdownPercent = 0m;

        foreach (var point in equityPoints)
        {
            if (point.Equity > peak)
            {
                peak = point.Equity;
            }

            var drawdown = peak - point.Equity;
            var drawdownPercent = peak > 0 ? (drawdown / peak) * 100 : 0m;

            if (drawdown > maxDrawdown)
            {
                maxDrawdown = drawdown;
                maxDrawdownPercent = drawdownPercent;
            }
        }

        return (maxDrawdown, maxDrawdownPercent);
    }

    /// <summary>
    /// Calculate Sortino Ratio (downside risk-adjusted return)
    /// Alternative to Sharpe that only considers downside volatility
    /// </summary>
    public static decimal CalculateSortinoRatio(
        List<BacktestTrade> trades,
        int tradingDays,
        decimal targetReturn = 0m)
    {
        if (trades.Count < 2)
            return 0m;

        var returns = trades.Select(t => t.RealizedPLPercent / 100).ToList();
        var avgReturn = returns.Average();

        // Calculate downside deviation (only negative returns)
        var downsideReturns = returns.Where(r => r < targetReturn).ToList();

        if (!downsideReturns.Any())
            return decimal.MaxValue; // No downside risk

        var downsideVariance = downsideReturns.Sum(r => (r - targetReturn) * (r - targetReturn)) / downsideReturns.Count;
        var downsideStdDev = (decimal)Math.Sqrt((double)downsideVariance);

        if (downsideStdDev == 0m)
            return decimal.MaxValue;

        // Annualize
        var tradesPerYear = tradingDays > 0
            ? (decimal)trades.Count / tradingDays * TradingDaysPerYear
            : TradingDaysPerYear;

        var annualizedAvgReturn = avgReturn * tradesPerYear;
        var annualizedDownsideStdDev = downsideStdDev * (decimal)Math.Sqrt((double)tradesPerYear);

        var sortinoRatio = annualizedDownsideStdDev > 0
            ? (annualizedAvgReturn - targetReturn) / annualizedDownsideStdDev
            : 0m;

        return sortinoRatio;
    }

    /// <summary>
    /// Calculate Calmar Ratio (return / max drawdown)
    /// </summary>
    public static decimal CalculateCalmarRatio(
        decimal annualizedReturn,
        decimal maxDrawdownPercent)
    {
        if (maxDrawdownPercent == 0m)
            return annualizedReturn > 0 ? decimal.MaxValue : 0m;

        return annualizedReturn / maxDrawdownPercent;
    }

    /// <summary>
    /// Calculate expectancy (average profit per trade)
    /// </summary>
    public static decimal CalculateExpectancy(List<BacktestTrade> trades)
    {
        if (!trades.Any())
            return 0m;

        var winRate = trades.Count(t => t.RealizedPL > 0) / (decimal)trades.Count;
        var avgWin = trades.Where(t => t.RealizedPL > 0).Any()
            ? trades.Where(t => t.RealizedPL > 0).Average(t => t.RealizedPL)
            : 0m;
        var avgLoss = trades.Where(t => t.RealizedPL < 0).Any()
            ? trades.Where(t => t.RealizedPL < 0).Average(t => Math.Abs(t.RealizedPL))
            : 0m;

        // Expectancy = (Win Rate × Avg Win) - (Loss Rate × Avg Loss)
        var expectancy = (winRate * avgWin) - ((1 - winRate) * avgLoss);
        return expectancy;
    }

    /// <summary>
    /// Calculate recovery factor (net profit / max drawdown)
    /// </summary>
    public static decimal CalculateRecoveryFactor(
        decimal netProfit,
        decimal maxDrawdown)
    {
        if (maxDrawdown == 0m)
            return netProfit > 0 ? decimal.MaxValue : 0m;

        return netProfit / maxDrawdown;
    }

    /// <summary>
    /// Calculate consecutive wins and losses
    /// </summary>
    public static (int maxConsecutiveWins, int maxConsecutiveLosses) CalculateConsecutiveWinsLosses(
        List<BacktestTrade> trades)
    {
        if (!trades.Any())
            return (0, 0);

        int currentWinStreak = 0;
        int currentLossStreak = 0;
        int maxWinStreak = 0;
        int maxLossStreak = 0;

        foreach (var trade in trades.OrderBy(t => t.ExitTime))
        {
            if (trade.RealizedPL > 0)
            {
                currentWinStreak++;
                currentLossStreak = 0;
                maxWinStreak = Math.Max(maxWinStreak, currentWinStreak);
            }
            else if (trade.RealizedPL < 0)
            {
                currentLossStreak++;
                currentWinStreak = 0;
                maxLossStreak = Math.Max(maxLossStreak, currentLossStreak);
            }
            else // Break even
            {
                currentWinStreak = 0;
                currentLossStreak = 0;
            }
        }

        return (maxWinStreak, maxLossStreak);
    }

    /// <summary>
    /// Calculate average holding period
    /// </summary>
    public static TimeSpan CalculateAverageHoldingPeriod(List<BacktestTrade> trades)
    {
        if (!trades.Any())
            return TimeSpan.Zero;

        var totalHours = trades.Sum(t => (t.ExitTime - t.EntryTime).TotalHours);
        var avgHours = totalHours / trades.Count;

        return TimeSpan.FromHours(avgHours);
    }

    /// <summary>
    /// Calculate equity curve statistics
    /// </summary>
    public static EquityCurveStats CalculateEquityCurveStats(List<EquityCurvePoint> equityCurve)
    {
        if (!equityCurve.Any())
        {
            return new EquityCurveStats
            {
                StartingEquity = 0,
                EndingEquity = 0,
                PeakEquity = 0,
                ValleyEquity = 0,
                AverageEquity = 0,
                EquityStdDev = 0
            };
        }

        var equities = equityCurve.Select(p => p.Equity).ToList();

        var startingEquity = equities.First();
        var endingEquity = equities.Last();
        var peakEquity = equities.Max();
        var valleyEquity = equities.Min();
        var avgEquity = equities.Average();

        // Standard deviation
        var variance = equities.Sum(e => (e - avgEquity) * (e - avgEquity)) / equities.Count;
        var stdDev = (decimal)Math.Sqrt((double)variance);

        return new EquityCurveStats
        {
            StartingEquity = startingEquity,
            EndingEquity = endingEquity,
            PeakEquity = peakEquity,
            ValleyEquity = valleyEquity,
            AverageEquity = avgEquity,
            EquityStdDev = stdDev
        };
    }
}

/// <summary>
/// Equity curve statistics
/// </summary>
public class EquityCurveStats
{
    public decimal StartingEquity { get; set; }
    public decimal EndingEquity { get; set; }
    public decimal PeakEquity { get; set; }
    public decimal ValleyEquity { get; set; }
    public decimal AverageEquity { get; set; }
    public decimal EquityStdDev { get; set; }
}
