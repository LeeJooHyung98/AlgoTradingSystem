namespace AlgoTrading.Desktop.Services;

/// <summary>
/// Interface for AlgoTrading API Client
/// Provides HTTP communication with the backend API
/// </summary>
public interface IAlgoTradingApiClient
{
    // Strategy endpoints
    Task<List<StrategyDto>> GetAvailableStrategiesAsync(CancellationToken cancellationToken = default);
    Task<StrategyDto> GetStrategyByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StrategyDto> CreateStrategyAsync(CreateStrategyRequest request, CancellationToken cancellationToken = default);
    Task<StrategyDto> ActivateStrategyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StrategyDto> DeactivateStrategyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SignalDto> GenerateSignalAsync(Guid strategyId, string stockCode, CancellationToken cancellationToken = default);

    // Account endpoints (to be implemented)
    Task<AccountSummaryDto> GetAccountSummaryAsync(CancellationToken cancellationToken = default);

    // Portfolio endpoints (to be implemented)
    Task<List<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default);

    // Order endpoints
    Task<List<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetOrdersByAccountAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateMarketOrderAsync(CreateMarketOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateLimitOrderAsync(CreateLimitOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelOrderAsync(Guid orderId, string? reason = null, CancellationToken cancellationToken = default);
}

// DTOs matching API contracts
public class StrategyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StrategyType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int RuleCount { get; set; }
    public Dictionary<string, object> DefaultParameters { get; set; } = new();
}

public class CreateStrategyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StrategyType { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}

public class SignalDto
{
    public bool HasSignal { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public string SignalStrength { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
}

public class AccountSummaryDto
{
    public decimal TotalAssets { get; set; }
    public decimal Cash { get; set; }
    public decimal StockValue { get; set; }
    public decimal TotalPnL { get; set; }
    public decimal TodayPnL { get; set; }
    public decimal DayPnLPercentage { get; set; }
}

public class PositionDto
{
    public string StockCode { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal ProfitLossPercentage { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int FilledQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public decimal? AverageFillPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Guid? StrategyId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? BrokerOrderId { get; set; }
    public string? CancellationReason { get; set; }
    public string? RejectionReason { get; set; }
}

public class CreateMarketOrderRequest
{
    public string StockCode { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty; // "Buy" or "Sell"
    public int Quantity { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public Guid? StrategyId { get; set; }
}

public class CreateLimitOrderRequest
{
    public string StockCode { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty; // "Buy" or "Sell"
    public int Quantity { get; set; }
    public decimal LimitPrice { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public Guid? StrategyId { get; set; }
}
