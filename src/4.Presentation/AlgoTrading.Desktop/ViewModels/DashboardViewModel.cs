using AlgoTrading.Desktop.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace AlgoTrading.Desktop.ViewModels;

/// <summary>
/// ViewModel for Dashboard View
/// Displays overview of account, strategies, and recent activity
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IAlgoTradingApiClient _apiClient;
    private readonly ITradingHubClient _hubClient;
    private readonly ILogger<DashboardViewModel> _logger;

    #region Properties

    // Connection Status
    private bool _isHubConnected;
    public bool IsHubConnected
    {
        get => _isHubConnected;
        set => SetProperty(ref _isHubConnected, value);
    }

    // Account Summary
    private decimal _totalAssets;
    public decimal TotalAssets
    {
        get => _totalAssets;
        set => SetProperty(ref _totalAssets, value);
    }

    private decimal _cash;
    public decimal Cash
    {
        get => _cash;
        set => SetProperty(ref _cash, value);
    }

    private decimal _stockValue;
    public decimal StockValue
    {
        get => _stockValue;
        set => SetProperty(ref _stockValue, value);
    }

    private decimal _totalPnL;
    public decimal TotalPnL
    {
        get => _totalPnL;
        set => SetProperty(ref _totalPnL, value);
    }

    private decimal _todayPnL;
    public decimal TodayPnL
    {
        get => _todayPnL;
        set => SetProperty(ref _todayPnL, value);
    }

    private decimal _dayPnLPercentage;
    public decimal DayPnLPercentage
    {
        get => _dayPnLPercentage;
        set => SetProperty(ref _dayPnLPercentage, value);
    }

    // Active Strategies Count
    private int _activeStrategiesCount;
    public int ActiveStrategiesCount
    {
        get => _activeStrategiesCount;
        set => SetProperty(ref _activeStrategiesCount, value);
    }

    // Open Positions Count
    private int _openPositionsCount;
    public int OpenPositionsCount
    {
        get => _openPositionsCount;
        set => SetProperty(ref _openPositionsCount, value);
    }

    // Today's Orders Count
    private int _todayOrdersCount;
    public int TodayOrdersCount
    {
        get => _todayOrdersCount;
        set => SetProperty(ref _todayOrdersCount, value);
    }

    // Recent Positions
    public ObservableCollection<PositionItemViewModel> RecentPositions { get; }

    // Recent Orders
    public ObservableCollection<OrderItemViewModel> RecentOrders { get; }

    #endregion

    public DashboardViewModel(
        IAlgoTradingApiClient apiClient,
        ITradingHubClient hubClient,
        ILogger<DashboardViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _hubClient = hubClient ?? throw new ArgumentNullException(nameof(hubClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        RecentPositions = new ObservableCollection<PositionItemViewModel>();
        RecentOrders = new ObservableCollection<OrderItemViewModel>();

        // Subscribe to SignalR events
        SubscribeToHubEvents();
    }

    /// <summary>
    /// Subscribe to SignalR hub events
    /// </summary>
    private void SubscribeToHubEvents()
    {
        // Connection status changes
        _hubClient.ConnectionStatusChanged += OnHubConnectionStatusChanged;

        // Position updates
        _hubClient.PositionUpdateReceived += OnPositionUpdateReceived;

        // Order updates
        _hubClient.OrderUpdateReceived += OnOrderUpdateReceived;

        // Execution updates
        _hubClient.ExecutionUpdateReceived += OnExecutionUpdateReceived;

        // Market data updates
        _hubClient.MarketDataUpdateReceived += OnMarketDataUpdateReceived;

        // Update initial connection status
        IsHubConnected = _hubClient.IsConnected;
    }

    /// <summary>
    /// Handle hub connection status changed
    /// </summary>
    private void OnHubConnectionStatusChanged(object? sender, bool isConnected)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            IsHubConnected = isConnected;
            _logger.LogInformation("Hub connection status: {Status}", isConnected ? "Connected" : "Disconnected");
        });
    }

    /// <summary>
    /// Handle position update received
    /// </summary>
    private void OnPositionUpdateReceived(object? sender, PositionUpdateEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(async () =>
        {
            _logger.LogDebug("Position update received, refreshing positions");
            await LoadRecentPositionsAsync();
        });
    }

    /// <summary>
    /// Handle order update received
    /// </summary>
    private void OnOrderUpdateReceived(object? sender, OrderUpdateEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(async () =>
        {
            _logger.LogDebug("Order update received, refreshing orders");
            await LoadRecentOrdersAsync();
        });
    }

    /// <summary>
    /// Handle execution update received
    /// </summary>
    private void OnExecutionUpdateReceived(object? sender, ExecutionUpdateEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(async () =>
        {
            _logger.LogDebug("Execution update received, refreshing data");
            await LoadAccountSummaryAsync();
            await LoadRecentPositionsAsync();
            await LoadRecentOrdersAsync();
        });
    }

    /// <summary>
    /// Handle market data update received
    /// </summary>
    private void OnMarketDataUpdateReceived(object? sender, MarketDataUpdateEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _logger.LogTrace("Market data update received for {StockCode}: {Price}",
                e.StockCode, e.CurrentPrice);
            // Update positions with latest prices if needed
            // This would be more sophisticated in production
        });
    }

    /// <summary>
    /// Initialize dashboard data
    /// </summary>
    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadAccountSummaryAsync();
        await LoadRecentPositionsAsync();
        await LoadRecentOrdersAsync();
        await LoadActiveStrategiesCountAsync();
    }

    /// <summary>
    /// Load account summary
    /// </summary>
    private async Task LoadAccountSummaryAsync()
    {
        try
        {
            _logger.LogInformation("Loading account summary");

            var summary = await _apiClient.GetAccountSummaryAsync();

            TotalAssets = summary.TotalAssets;
            Cash = summary.Cash;
            StockValue = summary.StockValue;
            TotalPnL = summary.TotalPnL;
            TodayPnL = summary.TodayPnL;
            DayPnLPercentage = summary.DayPnLPercentage;

            _logger.LogInformation("Account summary loaded: Total Assets = {TotalAssets}", TotalAssets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load account summary");
            MessageBox.Show($"Failed to load account summary: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Load recent positions
    /// </summary>
    private async Task LoadRecentPositionsAsync()
    {
        try
        {
            _logger.LogInformation("Loading recent positions");

            var positions = await _apiClient.GetPositionsAsync();

            RecentPositions.Clear();
            foreach (var position in positions.Take(5)) // Top 5 positions
            {
                RecentPositions.Add(new PositionItemViewModel(position));
            }

            OpenPositionsCount = positions.Count;

            _logger.LogInformation("Loaded {Count} positions", positions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load positions");
            MessageBox.Show($"Failed to load positions: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Load recent orders
    /// </summary>
    private async Task LoadRecentOrdersAsync()
    {
        try
        {
            _logger.LogInformation("Loading recent orders");

            var orders = await _apiClient.GetOrdersAsync();

            RecentOrders.Clear();
            foreach (var order in orders.Take(5).OrderByDescending(o => o.SubmittedAt))
            {
                RecentOrders.Add(new OrderItemViewModel(order));
            }

            TodayOrdersCount = orders.Count(o => o.SubmittedAt.Date == DateTime.Today);

            _logger.LogInformation("Loaded {Count} recent orders", orders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load orders");
            MessageBox.Show($"Failed to load orders: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Load active strategies count
    /// </summary>
    private async Task LoadActiveStrategiesCountAsync()
    {
        try
        {
            _logger.LogInformation("Loading active strategies count");

            var strategies = await _apiClient.GetAvailableStrategiesAsync();
            // Note: This endpoint returns available strategy types, not instances
            // TODO: Update when proper strategy instances endpoint is available
            ActiveStrategiesCount = strategies.Count;

            _logger.LogInformation("Active strategies count: {Count}", ActiveStrategiesCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load strategies count");
            // Don't show error for this non-critical data
        }
    }

    /// <summary>
    /// Refresh all dashboard data
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Refreshing dashboard...";

            await InitializeAsync();

            _logger.LogInformation("Dashboard refreshed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh dashboard");
            MessageBox.Show($"Failed to refresh dashboard: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }
}

/// <summary>
/// ViewModel for individual position item
/// </summary>
public class PositionItemViewModel : ViewModelBase
{
    public string StockCode { get; }
    public string StockName { get; }
    public int Quantity { get; }
    public decimal AveragePrice { get; }
    public decimal CurrentPrice { get; }
    public decimal ProfitLoss { get; }
    public decimal ProfitLossPercentage { get; }

    public bool IsProfit => ProfitLoss >= 0;
    public bool IsLoss => ProfitLoss < 0;

    public PositionItemViewModel(PositionDto position)
    {
        StockCode = position.StockCode;
        StockName = position.StockName;
        Quantity = position.Quantity;
        AveragePrice = position.AveragePrice;
        CurrentPrice = position.CurrentPrice;
        ProfitLoss = position.ProfitLoss;
        ProfitLossPercentage = position.ProfitLossPercentage;
    }
}

/// <summary>
/// ViewModel for individual order item
/// </summary>
public class OrderItemViewModel : ViewModelBase
{
    public Guid Id { get; }
    public string StockCode { get; }
    public string OrderType { get; }
    public string Side { get; }
    public int Quantity { get; }
    public decimal Price { get; }
    public string Status { get; }
    public DateTime CreatedAt { get; }

    public bool IsBuy => Side == "Buy";
    public bool IsSell => Side == "Sell";

    public OrderItemViewModel(OrderDto order)
    {
        Id = order.Id;
        StockCode = order.StockCode;
        OrderType = order.Type;
        Side = order.Side;
        Quantity = order.Quantity;
        Price = order.LimitPrice ?? order.AverageFillPrice ?? 0m;
        Status = order.Status;
        CreatedAt = order.SubmittedAt;
    }
}
