using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Desktop.Services;

/// <summary>
/// SignalR Trading Hub Client implementation
/// Manages real-time connection to the trading hub
/// </summary>
public class TradingHubClient : ITradingHubClient, IAsyncDisposable
{
    private readonly ILogger<TradingHubClient> _logger;
    private readonly HubConnection _hubConnection;

    public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

    public event EventHandler<bool>? ConnectionStatusChanged;
    public event EventHandler<OrderUpdateEventArgs>? OrderUpdateReceived;
    public event EventHandler<ExecutionUpdateEventArgs>? ExecutionUpdateReceived;
    public event EventHandler<PositionUpdateEventArgs>? PositionUpdateReceived;
    public event EventHandler<MarketDataUpdateEventArgs>? MarketDataUpdateReceived;

    public TradingHubClient(
        IConfiguration configuration,
        ILogger<TradingHubClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Get SignalR hub URL from configuration
        var apiBaseUrl = configuration["AlgoTradingApi:BaseUrl"] ?? "https://localhost:7001";
        var hubUrl = $"{apiBaseUrl}/hubs/trading";

        // Build SignalR connection
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
            .Build();

        // Register event handlers
        RegisterHubHandlers();

        // Handle connection state changes
        _hubConnection.Closed += OnConnectionClosed;
        _hubConnection.Reconnecting += OnReconnecting;
        _hubConnection.Reconnected += OnReconnected;

        _logger.LogInformation("TradingHubClient initialized with URL: {HubUrl}", hubUrl);
    }

    /// <summary>
    /// Connect to SignalR hub
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                _logger.LogInformation("Connecting to Trading Hub...");
                await _hubConnection.StartAsync(cancellationToken);
                _logger.LogInformation("Connected to Trading Hub");

                ConnectionStatusChanged?.Invoke(this, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Trading Hub");
            throw;
        }
    }

    /// <summary>
    /// Disconnect from SignalR hub
    /// </summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_hubConnection.State != HubConnectionState.Disconnected)
            {
                _logger.LogInformation("Disconnecting from Trading Hub...");
                await _hubConnection.StopAsync(cancellationToken);
                _logger.LogInformation("Disconnected from Trading Hub");

                ConnectionStatusChanged?.Invoke(this, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from Trading Hub");
            throw;
        }
    }

    /// <summary>
    /// Subscribe to account updates
    /// </summary>
    public async Task SubscribeToAccountAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Subscribing to account {AccountNumber}", accountNumber);
            await _hubConnection.InvokeAsync("SubscribeToAccount", accountNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to account {AccountNumber}", accountNumber);
            throw;
        }
    }

    /// <summary>
    /// Unsubscribe from account updates
    /// </summary>
    public async Task UnsubscribeFromAccountAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unsubscribing from account {AccountNumber}", accountNumber);
            await _hubConnection.InvokeAsync("UnsubscribeFromAccount", accountNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe from account {AccountNumber}", accountNumber);
            throw;
        }
    }

    /// <summary>
    /// Subscribe to stock updates
    /// </summary>
    public async Task SubscribeToStockAsync(string stockCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Subscribing to stock {StockCode}", stockCode);
            await _hubConnection.InvokeAsync("SubscribeToStock", stockCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to stock {StockCode}", stockCode);
            throw;
        }
    }

    /// <summary>
    /// Unsubscribe from stock updates
    /// </summary>
    public async Task UnsubscribeFromStockAsync(string stockCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unsubscribing from stock {StockCode}", stockCode);
            await _hubConnection.InvokeAsync("UnsubscribeFromStock", stockCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe from stock {StockCode}", stockCode);
            throw;
        }
    }

    /// <summary>
    /// Register SignalR hub event handlers
    /// </summary>
    private void RegisterHubHandlers()
    {
        // Order updates
        _hubConnection.On<object>("OrderUpdated", (orderData) =>
        {
            _logger.LogDebug("Order update received");
            OrderUpdateReceived?.Invoke(this, new OrderUpdateEventArgs
            {
                OrderData = orderData
            });
        });

        // Execution updates
        _hubConnection.On<object>("ExecutionReceived", (executionData) =>
        {
            _logger.LogDebug("Execution update received");
            ExecutionUpdateReceived?.Invoke(this, new ExecutionUpdateEventArgs
            {
                ExecutionData = executionData
            });
        });

        // Position updates
        _hubConnection.On<object>("PositionUpdated", (positionData) =>
        {
            _logger.LogDebug("Position update received");
            PositionUpdateReceived?.Invoke(this, new PositionUpdateEventArgs
            {
                PositionData = positionData
            });
        });

        // Market data updates
        _hubConnection.On<object>("MarketDataUpdated", (marketData) =>
        {
            _logger.LogDebug("Market data update received");
            // TODO: Parse marketData and extract fields
            MarketDataUpdateReceived?.Invoke(this, new MarketDataUpdateEventArgs
            {
                // Parse from marketData object
                Timestamp = DateTime.Now
            });
        });

        // Subscription confirmations
        _hubConnection.On<string>("SubscribedToAccount", (accountNumber) =>
        {
            _logger.LogInformation("Subscribed to account {AccountNumber}", accountNumber);
        });

        _hubConnection.On<string>("UnsubscribedFromAccount", (accountNumber) =>
        {
            _logger.LogInformation("Unsubscribed from account {AccountNumber}", accountNumber);
        });

        _hubConnection.On<string>("SubscribedToStock", (stockCode) =>
        {
            _logger.LogInformation("Subscribed to stock {StockCode}", stockCode);
        });

        _hubConnection.On<string>("UnsubscribedFromStock", (stockCode) =>
        {
            _logger.LogInformation("Unsubscribed from stock {StockCode}", stockCode);
        });
    }

    /// <summary>
    /// Handle connection closed
    /// </summary>
    private Task OnConnectionClosed(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Connection to Trading Hub closed with error");
        }
        else
        {
            _logger.LogInformation("Connection to Trading Hub closed");
        }

        ConnectionStatusChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle reconnecting
    /// </summary>
    private Task OnReconnecting(Exception? exception)
    {
        _logger.LogWarning(exception, "Reconnecting to Trading Hub...");
        ConnectionStatusChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle reconnected
    /// </summary>
    private Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation("Reconnected to Trading Hub with connection ID: {ConnectionId}", connectionId);
        ConnectionStatusChanged?.Invoke(this, true);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
