using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.ValueObjects;
using ErrorOr;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// Interface for Trading Strategy Management Service
/// Provides CRUD operations and lifecycle management for trading strategies
/// </summary>
public interface IStrategyService
{
    /// <summary>
    /// Create a new trading strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> CreateStrategyAsync(
        string name,
        string description,
        Core.Enums.StrategyType type,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategy by ID
    /// </summary>
    Task<ErrorOr<TradingStrategy>> GetStrategyByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategy with rules by ID
    /// </summary>
    Task<ErrorOr<TradingStrategy>> GetStrategyWithRulesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all strategies
    /// </summary>
    Task<ErrorOr<IEnumerable<TradingStrategy>>> GetAllStrategiesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active strategies
    /// </summary>
    Task<ErrorOr<IEnumerable<TradingStrategy>>> GetActiveStrategiesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategies by user
    /// </summary>
    Task<ErrorOr<IEnumerable<TradingStrategy>>> GetStrategiesByUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategies by type
    /// </summary>
    Task<ErrorOr<IEnumerable<TradingStrategy>>> GetStrategiesByTypeAsync(
        Core.Enums.StrategyType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> UpdateStrategyAsync(
        Guid id,
        string name,
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete strategy
    /// </summary>
    Task<ErrorOr<bool>> DeleteStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> ActivateStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> DeactivateStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set target stocks for strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> SetTargetStocksAsync(
        Guid id,
        List<string> stockCodes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set risk parameters for strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> SetRiskParametersAsync(
        Guid id,
        decimal maxPositionSize,
        decimal stopLoss,
        decimal takeProfit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add rule to strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> AddRuleAsync(
        Guid strategyId,
        string name,
        string description,
        Core.Enums.RuleType type,
        string condition,
        int priority,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove rule from strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> RemoveRuleAsync(
        Guid strategyId,
        Guid ruleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update strategy statistics after trade execution
    /// </summary>
    Task<ErrorOr<TradingStrategy>> UpdateStrategyStatisticsAsync(
        Guid strategyId,
        decimal profitLoss,
        bool isWinningTrade,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clone existing strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> CloneStrategyAsync(
        Guid sourceStrategyId,
        string newName,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate strategy configuration
    /// </summary>
    Task<ErrorOr<bool>> ValidateStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
