using AlgoTrading.Core.ValueObjects.Common;

namespace AlgoTrading.Core.ValueObjects.Strategy;

/// <summary>
/// Backtest Result Value Object
/// Represents comprehensive backtest performance metrics
/// </summary>
public sealed class BacktestResult : ValueObject
{
    /// <summary>
    /// Initial capital
    /// </summary>
    public Money InitialCapital { get; }

    /// <summary>
    /// Final capital
    /// </summary>
    public Money FinalCapital { get; }

    /// <summary>
    /// Total return amount
    /// </summary>
    public Money TotalReturn => FinalCapital - InitialCapital;

    /// <summary>
    /// Total return percentage
    /// </summary>
    public Percentage TotalReturnPercent { get; }

    /// <summary>
    /// Total number of trades
    /// </summary>
    public int TotalTrades { get; }

    /// <summary>
    /// Number of winning trades
    /// </summary>
    public int WinningTrades { get; }

    /// <summary>
    /// Number of losing trades
    /// </summary>
    public int LosingTrades { get; }

    /// <summary>
    /// Win rate percentage
    /// </summary>
    public Percentage WinRate { get; }

    /// <summary>
    /// Average win amount
    /// </summary>
    public Money? AverageWin { get; }

    /// <summary>
    /// Average loss amount
    /// </summary>
    public Money? AverageLoss { get; }

    /// <summary>
    /// Largest win
    /// </summary>
    public Money? LargestWin { get; }

    /// <summary>
    /// Largest loss
    /// </summary>
    public Money? LargestLoss { get; }

    /// <summary>
    /// Profit factor (gross profit / gross loss)
    /// </summary>
    public decimal? ProfitFactor { get; }

    /// <summary>
    /// Sharpe ratio
    /// </summary>
    public decimal? SharpeRatio { get; }

    /// <summary>
    /// Maximum drawdown amount
    /// </summary>
    public Money? MaxDrawdown { get; }

    /// <summary>
    /// Maximum drawdown percentage
    /// </summary>
    public Percentage? MaxDrawdownPercent { get; }

    /// <summary>
    /// Average trade duration
    /// </summary>
    public TimeSpan? AverageTradeDuration { get; }

    /// <summary>
    /// Backtest duration
    /// </summary>
    public TimeSpan BacktestDuration { get; }

    /// <summary>
    /// Number of trading days
    /// </summary>
    public int TradingDays { get; }

    private BacktestResult(
        Money initialCapital,
        Money finalCapital,
        Percentage totalReturnPercent,
        int totalTrades,
        int winningTrades,
        int losingTrades,
        Percentage winRate,
        Money? averageWin,
        Money? averageLoss,
        Money? largestWin,
        Money? largestLoss,
        decimal? profitFactor,
        decimal? sharpeRatio,
        Money? maxDrawdown,
        Percentage? maxDrawdownPercent,
        TimeSpan? averageTradeDuration,
        TimeSpan backtestDuration,
        int tradingDays)
    {
        InitialCapital = initialCapital;
        FinalCapital = finalCapital;
        TotalReturnPercent = totalReturnPercent;
        TotalTrades = totalTrades;
        WinningTrades = winningTrades;
        LosingTrades = losingTrades;
        WinRate = winRate;
        AverageWin = averageWin;
        AverageLoss = averageLoss;
        LargestWin = largestWin;
        LargestLoss = largestLoss;
        ProfitFactor = profitFactor;
        SharpeRatio = sharpeRatio;
        MaxDrawdown = maxDrawdown;
        MaxDrawdownPercent = maxDrawdownPercent;
        AverageTradeDuration = averageTradeDuration;
        BacktestDuration = backtestDuration;
        TradingDays = tradingDays;
    }

    /// <summary>
    /// Create a backtest result
    /// </summary>
    public static BacktestResult Create(
        Money initialCapital,
        Money finalCapital,
        int totalTrades,
        int winningTrades,
        int losingTrades,
        Money? averageWin = null,
        Money? averageLoss = null,
        Money? largestWin = null,
        Money? largestLoss = null,
        decimal? profitFactor = null,
        decimal? sharpeRatio = null,
        Money? maxDrawdown = null,
        Percentage? maxDrawdownPercent = null,
        TimeSpan? averageTradeDuration = null,
        TimeSpan? backtestDuration = null,
        int tradingDays = 0)
    {
        if (initialCapital.Amount <= 0)
            throw new ArgumentException("Initial capital must be positive", nameof(initialCapital));

        if (finalCapital.Amount < 0)
            throw new ArgumentException("Final capital cannot be negative", nameof(finalCapital));

        if (totalTrades < 0)
            throw new ArgumentException("Total trades cannot be negative", nameof(totalTrades));

        if (winningTrades < 0 || losingTrades < 0)
            throw new ArgumentException("Winning/Losing trades cannot be negative");

        if (winningTrades + losingTrades > totalTrades)
            throw new ArgumentException("Winning + Losing trades cannot exceed total trades");

        // Calculate metrics
        var totalReturnPercent = initialCapital.Amount > 0
            ? Percentage.FromDecimal((finalCapital.Amount - initialCapital.Amount) / initialCapital.Amount)
            : Percentage.Zero;

        var winRate = totalTrades > 0
            ? Percentage.FromFraction(winningTrades, totalTrades)
            : Percentage.Zero;

        var duration = backtestDuration ?? TimeSpan.Zero;

        return new BacktestResult(
            initialCapital,
            finalCapital,
            totalReturnPercent,
            totalTrades,
            winningTrades,
            losingTrades,
            winRate,
            averageWin,
            averageLoss,
            largestWin,
            largestLoss,
            profitFactor,
            sharpeRatio,
            maxDrawdown,
            maxDrawdownPercent,
            averageTradeDuration,
            duration,
            tradingDays
        );
    }

    /// <summary>
    /// Check if backtest was profitable
    /// </summary>
    public bool IsProfitable()
    {
        return FinalCapital > InitialCapital;
    }

    /// <summary>
    /// Check if backtest had losses
    /// </summary>
    public bool HasLosses()
    {
        return FinalCapital < InitialCapital;
    }

    /// <summary>
    /// Check if backtest broke even
    /// </summary>
    public bool IsBreakEven()
    {
        return FinalCapital == InitialCapital;
    }

    /// <summary>
    /// Get annualized return
    /// </summary>
    public Percentage? GetAnnualizedReturn()
    {
        if (TradingDays == 0)
            return null;

        // Assume 252 trading days per year
        var years = TradingDays / 252.0m;
        if (years <= 0)
            return null;

        var annualizedReturn = (decimal)Math.Pow((double)(FinalCapital.Amount / InitialCapital.Amount), (double)(1 / years)) - 1;
        return Percentage.FromDecimal(annualizedReturn);
    }

    /// <summary>
    /// Get average return per trade
    /// </summary>
    public Money? GetAverageReturnPerTrade()
    {
        if (TotalTrades == 0)
            return null;

        var totalReturn = FinalCapital - InitialCapital;
        return new Money(totalReturn.Amount / TotalTrades, InitialCapital.Currency);
    }

    /// <summary>
    /// Get profit/loss ratio
    /// </summary>
    public decimal? GetProfitLossRatio()
    {
        if (AverageWin == null || AverageLoss == null || AverageLoss.Amount == 0)
            return null;

        return Math.Abs(AverageWin.Amount / AverageLoss.Amount);
    }

    /// <summary>
    /// Get expectancy (average expected value per trade)
    /// </summary>
    public Money? GetExpectancy()
    {
        if (TotalTrades == 0)
            return null;

        var winProb = WinRate.DecimalValue;
        var lossProb = 1 - winProb;

        var avgWin = AverageWin?.Amount ?? 0;
        var avgLoss = AverageLoss?.Amount ?? 0;

        var expectancy = (winProb * avgWin) - (lossProb * Math.Abs(avgLoss));
        return new Money(expectancy, InitialCapital.Currency);
    }

    /// <summary>
    /// Get recovery factor (net profit / max drawdown)
    /// </summary>
    public decimal? GetRecoveryFactor()
    {
        if (MaxDrawdown == null || MaxDrawdown.Amount == 0)
            return null;

        var netProfit = TotalReturn.Amount;
        return netProfit / Math.Abs(MaxDrawdown.Amount);
    }

    /// <summary>
    /// Get Calmar ratio (annualized return / max drawdown)
    /// </summary>
    public decimal? GetCalmarRatio()
    {
        var annualizedReturn = GetAnnualizedReturn();
        if (annualizedReturn == null || MaxDrawdownPercent == null)
            return null;

        if (MaxDrawdownPercent.Value == 0)
            return null;

        return annualizedReturn.Value / MaxDrawdownPercent.Value;
    }

    /// <summary>
    /// Check if result meets minimum criteria
    /// </summary>
    public bool MeetsCriteria(
        Percentage? minWinRate = null,
        decimal? minProfitFactor = null,
        Percentage? maxDrawdown = null,
        int? minTrades = null)
    {
        if (minWinRate != null && WinRate < minWinRate)
            return false;

        if (minProfitFactor.HasValue && (ProfitFactor == null || ProfitFactor < minProfitFactor.Value))
            return false;

        if (maxDrawdown != null && MaxDrawdownPercent != null && MaxDrawdownPercent > maxDrawdown)
            return false;

        if (minTrades.HasValue && TotalTrades < minTrades.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Get performance grade (A-F)
    /// </summary>
    public string GetPerformanceGrade()
    {
        // Grading based on multiple factors
        var score = 0;

        // Win rate (max 25 points)
        if (WinRate.Value >= 60) score += 25;
        else if (WinRate.Value >= 50) score += 20;
        else if (WinRate.Value >= 40) score += 15;
        else if (WinRate.Value >= 30) score += 10;

        // Profit factor (max 25 points)
        if (ProfitFactor >= 2.0m) score += 25;
        else if (ProfitFactor >= 1.5m) score += 20;
        else if (ProfitFactor >= 1.2m) score += 15;
        else if (ProfitFactor >= 1.0m) score += 10;

        // Return (max 25 points)
        if (TotalReturnPercent.Value >= 50) score += 25;
        else if (TotalReturnPercent.Value >= 30) score += 20;
        else if (TotalReturnPercent.Value >= 15) score += 15;
        else if (TotalReturnPercent.Value >= 5) score += 10;

        // Max drawdown (max 25 points)
        if (MaxDrawdownPercent == null) score += 12;
        else if (MaxDrawdownPercent.Value <= 10) score += 25;
        else if (MaxDrawdownPercent.Value <= 20) score += 20;
        else if (MaxDrawdownPercent.Value <= 30) score += 15;
        else if (MaxDrawdownPercent.Value <= 40) score += 10;

        return score switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B",
            >= 60 => "C",
            >= 50 => "D",
            _ => "F"
        };
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"Return: {TotalReturnPercent}, Win Rate: {WinRate}, Trades: {TotalTrades}, Grade: {GetPerformanceGrade()}";
    }

    /// <summary>
    /// Detailed string representation
    /// </summary>
    public string ToDetailedString()
    {
        return $@"
Backtest Results:
- Initial Capital: {InitialCapital}
- Final Capital: {FinalCapital}
- Total Return: {TotalReturn} ({TotalReturnPercent})
- Total Trades: {TotalTrades}
- Win Rate: {WinRate} ({WinningTrades}W / {LosingTrades}L)
- Profit Factor: {ProfitFactor:F2}
- Sharpe Ratio: {SharpeRatio:F2}
- Max Drawdown: {MaxDrawdown} ({MaxDrawdownPercent})
- Performance Grade: {GetPerformanceGrade()}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return InitialCapital;
        yield return FinalCapital;
        yield return TotalTrades;
        yield return WinningTrades;
        yield return LosingTrades;
    }
}
