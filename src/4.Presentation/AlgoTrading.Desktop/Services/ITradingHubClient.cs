namespace AlgoTrading.Desktop.Services;

/// <summary>
/// Interface for SignalR Trading Hub Client
/// Provides real-time updates for orders, positions, and market data
/// </summary>
public interface ITradingHubClient
{
    /// <summary>
    /// Connection state
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connection status changed event
    /// </summary>
    event EventHandler<bool>? ConnectionStatusChanged;

    /// <summary>
    /// Order update received event
    /// </summary>
    event EventHandler<OrderUpdateEventArgs>? OrderUpdateReceived;

    /// <summary>
    /// Execution update received event
    /// </summary>
    event EventHandler<ExecutionUpdateEventArgs>? ExecutionUpdateReceived;

    /// <summary>
    /// Position update received event
    /// </summary>
    event EventHandler<PositionUpdateEventArgs>? PositionUpdateReceived;

    /// <summary>
    /// Market data update received event
    /// </summary>
    event EventHandler<MarketDataUpdateEventArgs>? MarketDataUpdateReceived;

    /// <summary>
    /// Connect to SignalR hub
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from SignalR hub
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to account updates
    /// </summary>
    Task SubscribeToAccountAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribe from account updates
    /// </summary>
    Task UnsubscribeFromAccountAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to stock updates
    /// </summary>
    Task SubscribeToStockAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribe from stock updates
    /// </summary>
    Task UnsubscribeFromStockAsync(string stockCode, CancellationToken cancellationToken = default);
}

/// <summary>
/// Order update event arguments
/// </summary>
public class OrderUpdateEventArgs : EventArgs
{
    public string AccountNumber { get; set; } = string.Empty;
    public object OrderData { get; set; } = new();
}

/// <summary>
/// Execution update event arguments
/// </summary>
public class ExecutionUpdateEventArgs : EventArgs
{
    public string AccountNumber { get; set; } = string.Empty;
    public object ExecutionData { get; set; } = new();
}

/// <summary>
/// Position update event arguments
/// </summary>
public class PositionUpdateEventArgs : EventArgs
{
    public string AccountNumber { get; set; } = string.Empty;
    public object PositionData { get; set; } = new();
}

/// <summary>
/// Market data update event arguments
/// </summary>
public class MarketDataUpdateEventArgs : EventArgs
{
    public string StockCode { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal ChangeRate { get; set; }
    public long Volume { get; set; }
    public DateTime Timestamp { get; set; }
}
