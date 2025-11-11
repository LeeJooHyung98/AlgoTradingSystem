using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Core.ValueObjects;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// Trading Strategy Management Service Implementation
/// Provides comprehensive strategy lifecycle management
/// </summary>
public class StrategyService : IStrategyService
{
    private readonly ILogger<StrategyService> _logger;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StrategyService(
        ILogger<StrategyService> logger,
        IStrategyRepository strategyRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategyRepository = strategyRepository ?? throw new ArgumentNullException(nameof(strategyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ErrorOr<TradingStrategy>> CreateStrategyAsync(
        string name,
        string description,
        Core.Enums.StrategyType type,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating new strategy: Name={Name}, Type={Type}, CreatedBy={CreatedBy}",
            name,
            type,
            createdBy);

        try
        {
            // Validate strategy name uniqueness
            var existingStrategy = await _strategyRepository.GetByNameAsync(name, cancellationToken);
            if (existingStrategy != null)
            {
                _logger.LogWarning("Strategy with name {Name} already exists", name);
                return Error.Conflict("Strategy.DuplicateName", $"Strategy with name '{name}' already exists");
            }

            // Create new strategy (StrategyType from entity namespace)
            var strategy = TradingStrategy.Create(name, description, (StrategyType)type, createdBy);

            // Add to repository
            await _strategyRepository.AddAsync(strategy, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy created successfully: Id={Id}, Name={Name}", strategy.Id, name);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating strategy: Name={Name}", name);
            return Error.Failure("Strategy.CreateFailed", "Failed to create strategy");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> GetStrategyByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strategy by ID: {Id}", id);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(id), cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", id);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategy: Id={Id}", id);
            return Error.Failure("Strategy.GetFailed", "Failed to retrieve strategy");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> GetStrategyWithRulesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strategy with rules: Id={Id}", id);

        try
        {
            var strategy = await _strategyRepository.GetWithRulesAsync(id, cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", id);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategy with rules: Id={Id}", id);
            return Error.Failure("Strategy.GetFailed", "Failed to retrieve strategy");
        }
    }

    public async Task<ErrorOr<IEnumerable<TradingStrategy>>> GetAllStrategiesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all strategies");

        try
        {
            var strategies = await _strategyRepository.GetAllAsync(cancellationToken);
            return strategies.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all strategies");
            return Error.Failure("Strategy.GetAllFailed", "Failed to retrieve strategies");
        }
    }

    public async Task<ErrorOr<IEnumerable<TradingStrategy>>> GetActiveStrategiesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting active strategies");

        try
        {
            var strategies = await _strategyRepository.GetActiveStrategiesAsync(cancellationToken);
            return strategies.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active strategies");
            return Error.Failure("Strategy.GetActiveFailed", "Failed to retrieve active strategies");
        }
    }

    public async Task<ErrorOr<IEnumerable<TradingStrategy>>> GetStrategiesByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strategies for user: {UserId}", userId);

        try
        {
            var strategies = await _strategyRepository.GetByCreatedByAsync(userId, cancellationToken);
            return strategies.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategies for user: {UserId}", userId);
            return Error.Failure("Strategy.GetByUserFailed", "Failed to retrieve user strategies");
        }
    }

    public async Task<ErrorOr<IEnumerable<TradingStrategy>>> GetStrategiesByTypeAsync(
        Core.Enums.StrategyType type,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting strategies by type: {Type}", type);

        try
        {
            var strategies = await _strategyRepository.GetByTypeAsync((StrategyType)type, cancellationToken);
            return strategies.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategies by type: {Type}", type);
            return Error.Failure("Strategy.GetByTypeFailed", "Failed to retrieve strategies by type");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> UpdateStrategyAsync(
        Guid id,
        string name,
        string description,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating strategy: Id={Id}", id);

        try
        {
            // Note: Strategy name and description cannot be updated after creation
            // This is by design to maintain strategy integrity
            // To change a strategy, clone it with a new name

            return Error.Validation(
                "Strategy.CannotUpdate",
                "Strategy name and description cannot be updated. Please clone the strategy instead.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating strategy: Id={Id}", id);
            return Error.Failure("Strategy.UpdateFailed", "Failed to update strategy");
        }
    }

    public async Task<ErrorOr<bool>> DeleteStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting strategy: Id={Id}", id);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(id), cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", id);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            // Cannot delete active strategy
            if (strategy.CanTradeNow())
            {
                _logger.LogWarning("Cannot delete active strategy: Id={Id}", id);
                return Error.Validation("Strategy.CannotDeleteActive", "Cannot delete active strategy");
            }

            _strategyRepository.Remove(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy deleted successfully: Id={Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting strategy: Id={Id}", id);
            return Error.Failure("Strategy.DeleteFailed", "Failed to delete strategy");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> ActivateStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating strategy: Id={Id}", id);

        try
        {
            var strategy = await _strategyRepository.GetWithRulesAsync(id, cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", id);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            // Validate strategy before activation
            var validationResult = await ValidateStrategyAsync(id, cancellationToken);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // Activate strategy
            strategy.Activate();

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy activated successfully: Id={Id}", id);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating strategy: Id={Id}", id);
            return Error.Failure("Strategy.ActivateFailed", "Failed to activate strategy");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> DeactivateStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating strategy: Id={Id}", id);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(id), cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", id);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            // Deactivate strategy
            strategy.Deactivate();

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy deactivated successfully: Id={Id}", id);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating strategy: Id={Id}", id);
            return Error.Failure("Strategy.DeactivateFailed", "Failed to deactivate strategy");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> SetTargetStocksAsync(
        Guid id,
        List<string> stockCodes,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting target stocks for strategy: Id={Id}, StockCount={Count}", id, stockCodes.Count);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(id), cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", id);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            // Convert stock codes to StockCode value objects
            var stocks = stockCodes.Select(code => new StockCode(code)).ToList();

            // Set target stocks
            strategy.SetTargetStocks(stocks);

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Target stocks set successfully for strategy: Id={Id}", id);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting target stocks: Id={Id}", id);
            return Error.Failure("Strategy.SetTargetStocksFailed", "Failed to set target stocks");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> SetRiskParametersAsync(
        Guid id,
        decimal maxPositionSize,
        decimal stopLoss,
        decimal takeProfit,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Setting risk parameters for strategy: Id={Id}, MaxPosition={MaxPosition}, StopLoss={StopLoss}, TakeProfit={TakeProfit}",
            id,
            maxPositionSize,
            stopLoss,
            takeProfit);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(id), cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", id);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            // Set risk parameters
            strategy.SetRiskParameters(maxPositionSize, stopLoss, takeProfit);

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Risk parameters set successfully for strategy: Id={Id}", id);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting risk parameters: Id={Id}", id);
            return Error.Failure("Strategy.SetRiskParametersFailed", "Failed to set risk parameters");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> AddRuleAsync(
        Guid strategyId,
        string name,
        string description,
        Core.Enums.RuleType type,
        string condition,
        int priority,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding rule to strategy: StrategyId={StrategyId}, RuleName={RuleName}", strategyId, name);

        try
        {
            var strategy = await _strategyRepository.GetWithRulesAsync(strategyId, cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", strategyId);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            // Create and add rule (cast enum type)
            var rule = StrategyRule.Create(name, description, (RuleType)type, condition, priority);
            strategy.AddRule(rule);

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rule added successfully to strategy: StrategyId={StrategyId}, RuleId={RuleId}", strategyId, rule.Id);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding rule to strategy: StrategyId={StrategyId}", strategyId);
            return Error.Failure("Strategy.AddRuleFailed", "Failed to add rule to strategy");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> RemoveRuleAsync(
        Guid strategyId,
        Guid ruleId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing rule from strategy: StrategyId={StrategyId}, RuleId={RuleId}", strategyId, ruleId);

        try
        {
            var strategy = await _strategyRepository.GetWithRulesAsync(strategyId, cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", strategyId);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            // Find and remove rule
            var rule = strategy.Rules.FirstOrDefault(r => r.Id.Value == ruleId);
            if (rule == null)
            {
                _logger.LogWarning("Rule not found: RuleId={RuleId}", ruleId);
                return Error.NotFound("Rule.NotFound", $"Rule with ID {ruleId} not found");
            }

            // RemoveRule expects StrategyRuleId
            strategy.RemoveRule(rule.Id);

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rule removed successfully from strategy: StrategyId={StrategyId}, RuleId={RuleId}", strategyId, ruleId);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing rule from strategy: StrategyId={StrategyId}, RuleId={RuleId}", strategyId, ruleId);
            return Error.Failure("Strategy.RemoveRuleFailed", "Failed to remove rule from strategy");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> UpdateStrategyStatisticsAsync(
        Guid strategyId,
        decimal profitLoss,
        bool isWinningTrade,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating strategy statistics: StrategyId={StrategyId}, PnL={PnL}, IsWin={IsWin}",
            strategyId, profitLoss, isWinningTrade);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);

            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: Id={Id}", strategyId);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            // Update performance (note: parameter order is isWin first, then profitLoss)
            strategy.UpdatePerformance(isWinningTrade, profitLoss);

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Strategy statistics updated: StrategyId={StrategyId}, TotalTrades={TotalTrades}, WinRate={WinRate}",
                strategyId, strategy.TotalTrades, strategy.WinRate);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating strategy statistics: StrategyId={StrategyId}", strategyId);
            return Error.Failure("Strategy.UpdateStatisticsFailed", "Failed to update strategy statistics");
        }
    }

    public async Task<ErrorOr<TradingStrategy>> CloneStrategyAsync(
        Guid sourceStrategyId,
        string newName,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cloning strategy: SourceId={SourceId}, NewName={NewName}", sourceStrategyId, newName);

        try
        {
            var sourceStrategy = await _strategyRepository.GetWithRulesAsync(sourceStrategyId, cancellationToken);

            if (sourceStrategy == null)
            {
                _logger.LogWarning("Source strategy not found: Id={Id}", sourceStrategyId);
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {sourceStrategyId} not found");
            }

            // Check name uniqueness
            var existingStrategy = await _strategyRepository.GetByNameAsync(newName, cancellationToken);
            if (existingStrategy != null)
            {
                return Error.Conflict("Strategy.DuplicateName", $"Strategy with name '{newName}' already exists");
            }

            // Create cloned strategy
            var clonedStrategy = TradingStrategy.Create(
                newName,
                $"Cloned from: {sourceStrategy.Name}",
                sourceStrategy.Type,
                createdBy);

            // Copy target stocks
            clonedStrategy.SetTargetStocks(sourceStrategy.TargetStocks.ToList());

            // Copy risk parameters
            clonedStrategy.SetRiskParameters(
                sourceStrategy.MaxPositionSizePercent,
                sourceStrategy.StopLossPercent,
                sourceStrategy.TakeProfitPercent);

            // Copy rules
            foreach (var rule in sourceStrategy.Rules)
            {
                var clonedRule = StrategyRule.Create(
                    rule.Name,
                    rule.Description,
                    rule.Type,
                    rule.Condition,
                    rule.Priority);
                clonedStrategy.AddRule(clonedRule);
            }

            await _strategyRepository.AddAsync(clonedStrategy, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy cloned successfully: NewId={NewId}, NewName={NewName}", clonedStrategy.Id, newName);

            return clonedStrategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning strategy: SourceId={SourceId}", sourceStrategyId);
            return Error.Failure("Strategy.CloneFailed", "Failed to clone strategy");
        }
    }

    public async Task<ErrorOr<bool>> ValidateStrategyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating strategy: Id={Id}", id);

        try
        {
            var strategy = await _strategyRepository.GetWithRulesAsync(id, cancellationToken);

            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {id} not found");
            }

            // Validation rules
            var errors = new List<Error>();

            // Must have at least one rule
            if (!strategy.Rules.Any())
            {
                errors.Add(Error.Validation("Strategy.NoRules", "Strategy must have at least one rule"));
            }

            // Must have at least one entry rule
            var hasEntryRule = strategy.Rules.Any(r =>
                r.IsEnabled &&
                (r.Type == RuleType.EntryLong || r.Type == RuleType.EntryShort));

            if (!hasEntryRule)
            {
                errors.Add(Error.Validation("Strategy.NoEntryRule", "Strategy must have at least one enabled entry rule"));
            }

            // Risk parameters must be valid
            if (strategy.MaxPositionSizePercent <= 0 || strategy.MaxPositionSizePercent > 100)
            {
                errors.Add(Error.Validation("Strategy.InvalidMaxPosition", "Max position size must be between 0 and 100"));
            }

            if (strategy.StopLossPercent < 0)
            {
                errors.Add(Error.Validation("Strategy.InvalidStopLoss", "Stop loss percent cannot be negative"));
            }

            if (strategy.TakeProfitPercent < 0)
            {
                errors.Add(Error.Validation("Strategy.InvalidTakeProfit", "Take profit percent cannot be negative"));
            }

            if (errors.Any())
            {
                _logger.LogWarning("Strategy validation failed: Id={Id}, ErrorCount={ErrorCount}", id, errors.Count);
                return errors;
            }

            _logger.LogDebug("Strategy validation passed: Id={Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating strategy: Id={Id}", id);
            return Error.Failure("Strategy.ValidationFailed", "Failed to validate strategy");
        }
    }
}
