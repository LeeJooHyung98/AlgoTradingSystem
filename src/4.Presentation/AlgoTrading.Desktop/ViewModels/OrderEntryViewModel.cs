using AlgoTrading.Desktop.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace AlgoTrading.Desktop.ViewModels;

/// <summary>
/// ViewModel for Order Entry View
/// Handles market and limit order creation
/// </summary>
public partial class OrderEntryViewModel : ViewModelBase
{
    private readonly IAlgoTradingApiClient _apiClient;
    private readonly ILogger<OrderEntryViewModel> _logger;

    #region Properties

    // Order Type Selection
    private bool _isMarketOrder = true;
    public bool IsMarketOrder
    {
        get => _isMarketOrder;
        set
        {
            SetProperty(ref _isMarketOrder, value);
            IsLimitOrder = !value;
            OnPropertyChanged(nameof(IsLimitPriceEnabled));
        }
    }

    private bool _isLimitOrder;
    public bool IsLimitOrder
    {
        get => _isLimitOrder;
        set => SetProperty(ref _isLimitOrder, value);
    }

    public bool IsLimitPriceEnabled => IsLimitOrder;

    // Order Side Selection
    private bool _isBuy = true;
    public bool IsBuy
    {
        get => _isBuy;
        set
        {
            SetProperty(ref _isBuy, value);
            IsSell = !value;
        }
    }

    private bool _isSell;
    public bool IsSell
    {
        get => _isSell;
        set => SetProperty(ref _isSell, value);
    }

    // Order Details
    private string _stockCode = string.Empty;
    public string StockCode
    {
        get => _stockCode;
        set => SetProperty(ref _stockCode, value);
    }

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    private decimal _limitPrice;
    public decimal LimitPrice
    {
        get => _limitPrice;
        set => SetProperty(ref _limitPrice, value);
    }

    // Account Selection
    private string _selectedAccount = string.Empty;
    public string SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    public ObservableCollection<string> AvailableAccounts { get; }

    // Recent Orders
    public ObservableCollection<OrderItemViewModel> RecentOrders { get; }

    #endregion

    public OrderEntryViewModel(
        IAlgoTradingApiClient apiClient,
        ILogger<OrderEntryViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        AvailableAccounts = new ObservableCollection<string>
        {
            "1234567890", // TODO: Load from configuration or API
            "0987654321"
        };

        RecentOrders = new ObservableCollection<OrderItemViewModel>();

        // Set default account
        if (AvailableAccounts.Count > 0)
            SelectedAccount = AvailableAccounts[0];
    }

    /// <summary>
    /// Initialize view - load recent orders
    /// </summary>
    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadRecentOrdersAsync();
    }

    /// <summary>
    /// Submit order to API
    /// </summary>
    [RelayCommand]
    private async Task SubmitOrderAsync()
    {
        if (!ValidateOrderInput())
            return;

        try
        {
            IsBusy = true;
            BusyMessage = "Submitting order...";

            OrderDto? createdOrder = null;

            if (IsMarketOrder)
            {
                _logger.LogInformation("Creating market order: {StockCode} {Side} {Quantity}",
                    StockCode, IsBuy ? "Buy" : "Sell", Quantity);

                var request = new CreateMarketOrderRequest
                {
                    StockCode = StockCode,
                    Side = IsBuy ? "Buy" : "Sell",
                    Quantity = Quantity,
                    AccountNumber = SelectedAccount
                };

                createdOrder = await _apiClient.CreateMarketOrderAsync(request);
            }
            else
            {
                _logger.LogInformation("Creating limit order: {StockCode} {Side} {Quantity} @ {Price}",
                    StockCode, IsBuy ? "Buy" : "Sell", Quantity, LimitPrice);

                var request = new CreateLimitOrderRequest
                {
                    StockCode = StockCode,
                    Side = IsBuy ? "Buy" : "Sell",
                    Quantity = Quantity,
                    LimitPrice = LimitPrice,
                    AccountNumber = SelectedAccount
                };

                createdOrder = await _apiClient.CreateLimitOrderAsync(request);
            }

            MessageBox.Show(
                $"Order submitted successfully!\n\n" +
                $"Order ID: {createdOrder.Id}\n" +
                $"Stock: {createdOrder.StockCode}\n" +
                $"Status: {createdOrder.Status}",
                "Order Submitted",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _logger.LogInformation("Order submitted successfully: {OrderId}", createdOrder.Id);

            // Reset form
            ResetForm();

            // Refresh recent orders
            await LoadRecentOrdersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit order");
            MessageBox.Show(
                $"Failed to submit order:\n\n{ex.Message}",
                "Order Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Cancel selected order
    /// </summary>
    [RelayCommand]
    private async Task CancelOrderAsync(OrderItemViewModel? orderItem)
    {
        if (orderItem == null)
            return;

        try
        {
            var result = MessageBox.Show(
                $"Are you sure you want to cancel this order?\n\n" +
                $"Stock: {orderItem.StockCode}\n" +
                $"Side: {orderItem.Side}\n" +
                $"Quantity: {orderItem.Quantity}",
                "Cancel Order",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsBusy = true;
            BusyMessage = "Cancelling order...";

            _logger.LogInformation("Cancelling order {OrderId}", orderItem.Id);

            await _apiClient.CancelOrderAsync(orderItem.Id, "Cancelled by user");

            MessageBox.Show(
                "Order cancelled successfully!",
                "Order Cancelled",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _logger.LogInformation("Order cancelled successfully: {OrderId}", orderItem.Id);

            // Refresh recent orders
            await LoadRecentOrdersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order {OrderId}", orderItem.Id);
            MessageBox.Show(
                $"Failed to cancel order:\n\n{ex.Message}",
                "Cancel Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Load recent orders for selected account
    /// </summary>
    private async Task LoadRecentOrdersAsync()
    {
        if (string.IsNullOrEmpty(SelectedAccount))
            return;

        try
        {
            _logger.LogInformation("Loading recent orders for account {Account}", SelectedAccount);

            var orders = await _apiClient.GetOrdersByAccountAsync(SelectedAccount);

            RecentOrders.Clear();
            foreach (var order in orders.Take(10).OrderByDescending(o => o.SubmittedAt))
            {
                RecentOrders.Add(new OrderItemViewModel(order));
            }

            _logger.LogInformation("Loaded {Count} recent orders", orders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent orders");
            // Don't show error dialog for background refresh
        }
    }

    /// <summary>
    /// Validate order input
    /// </summary>
    private bool ValidateOrderInput()
    {
        if (string.IsNullOrWhiteSpace(StockCode))
        {
            MessageBox.Show("Please enter stock code", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (Quantity <= 0)
        {
            MessageBox.Show("Quantity must be greater than 0", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (IsLimitOrder && LimitPrice <= 0)
        {
            MessageBox.Show("Limit price must be greater than 0", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedAccount))
        {
            MessageBox.Show("Please select an account", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Reset form to default values
    /// </summary>
    private void ResetForm()
    {
        StockCode = string.Empty;
        Quantity = 1;
        LimitPrice = 0;
        IsMarketOrder = true;
        IsBuy = true;
    }

    /// <summary>
    /// Refresh recent orders
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Refreshing orders...";

            await LoadRecentOrdersAsync();

            _logger.LogInformation("Orders refreshed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh orders");
            MessageBox.Show($"Failed to refresh orders: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }
}
