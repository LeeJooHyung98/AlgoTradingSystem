using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// Trading Strategy Interface
/// </summary>
public interface ITradingStrategy
{
    /// <summary>
    /// Strategy name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Strategy description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Strategy type
    /// </summary>
    StrategyType StrategyType { get; }

    /// <summary>
    /// Generate entry signal
    /// </summary>
    Task<SignalResult> GenerateEntrySignalAsync(
        string stockCode,
        List<PriceData> priceData,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate exit signal
    /// </summary>
    Task<SignalResult> GenerateExitSignalAsync(
        string stockCode,
        List<PriceData> priceData,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate strategy parameters
    /// </summary>
    bool ValidateParameters(Dictionary<string, object> parameters);

    /// <summary>
    /// Get default parameters
    /// </summary>
    Dictionary<string, object> GetDefaultParameters();
}

/// <summary>
/// Signal result
/// </summary>
public class SignalResult
{
    /// <summary>
    /// Signal generated
    /// </summary>
    public bool HasSignal { get; set; }

    /// <summary>
    /// Signal strength (0-100)
    /// </summary>
    public decimal SignalStrength { get; set; }

    /// <summary>
    /// Signal reason/description
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Suggested entry price
    /// </summary>
    public decimal? SuggestedPrice { get; set; }

    /// <summary>
    /// Suggested position size
    /// </summary>
    public decimal? SuggestedQuantity { get; set; }

    /// <summary>
    /// Stop loss price
    /// </summary>
    public decimal? StopLoss { get; set; }

    /// <summary>
    /// Take profit price
    /// </summary>
    public decimal? TakeProfit { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
