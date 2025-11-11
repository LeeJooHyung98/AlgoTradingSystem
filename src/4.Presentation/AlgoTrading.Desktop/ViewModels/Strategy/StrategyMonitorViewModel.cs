using AlgoTrading.Application.Services.Strategy;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace AlgoTrading.Desktop.ViewModels.Strategy;

/// <summary>
/// ViewModel for Strategy Monitor View
/// Real-time monitoring of active strategies
/// </summary>
public partial class StrategyMonitorViewModel : ViewModelBase
{
    private readonly IStrategyService _strategyService;
    private readonly ILogger<StrategyMonitorViewModel> _logger;
    private readonly System.Timers.Timer _refreshTimer;

    public ObservableCollection<ActiveStrategyViewModel> ActiveStrategies { get; }

    private bool _autoRefresh = true;
    public bool AutoRefresh
    {
        get => _autoRefresh;
        set
        {
            SetProperty(ref _autoRefresh, value);
            UpdateTimerState();
        }
    }

    private int _refreshInterval = 5; // seconds
    public int RefreshInterval
    {
        get => _refreshInterval;
        set
        {
            SetProperty(ref _refreshInterval, value);
            UpdateTimerState();
        }
    }

    private DateTime _lastRefresh;
    public DateTime LastRefresh
    {
        get => _lastRefresh;
        set => SetProperty(ref _lastRefresh, value);
    }

    public StrategyMonitorViewModel(
        IStrategyService strategyService,
        ILogger<StrategyMonitorViewModel> logger)
    {
        _strategyService = strategyService ?? throw new ArgumentNullException(nameof(strategyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ActiveStrategies = new ObservableCollection<ActiveStrategyViewModel>();

        _refreshTimer = new System.Timers.Timer();
        _refreshTimer.Elapsed += async (s, e) => await RefreshDataAsync();
        UpdateTimerState();
    }

    /// <summary>
    /// Load active strategies
    /// </summary>
    [RelayCommand]
    private async Task LoadActiveStrategiesAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading active strategies...";

            var result = await _strategyService.GetActiveStrategiesAsync();

            if (result.IsError)
            {
                _logger.LogError("Failed to load active strategies: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to load active strategies: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ActiveStrategies.Clear();
            foreach (var strategy in result.Value)
            {
                ActiveStrategies.Add(new ActiveStrategyViewModel
                {
                    Id = strategy.Id.Value,
                    Name = strategy.Name,
                    Type = strategy.Type.ToString(),
                    Status = strategy.Status.ToString(),
                    TotalTrades = strategy.TotalTrades,
                    WinningTrades = strategy.WinningTrades,
                    LosingTrades = strategy.LosingTrades,
                    WinRate = strategy.WinRate,
                    TotalPnL = strategy.TotalProfitLoss,
                    ActivatedAt = strategy.LastActivatedAt ?? DateTime.MinValue,
                    LastSignalAt = strategy.Signals.LastOrDefault()?.GeneratedAt
                });
            }

            LastRefresh = DateTime.Now;
            _logger.LogInformation("Loaded {Count} active strategies", ActiveStrategies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading active strategies");
            MessageBox.Show($"Error loading active strategies: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Refresh data (for timer)
    /// </summary>
    private async Task RefreshDataAsync()
    {
        if (!IsBusy && AutoRefresh)
        {
            await LoadActiveStrategiesAsync();
        }
    }

    /// <summary>
    /// Manual refresh
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadActiveStrategiesAsync();
    }

    /// <summary>
    /// Start monitoring
    /// </summary>
    [RelayCommand]
    private void StartMonitoring()
    {
        AutoRefresh = true;
        MessageBox.Show("Monitoring started", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Stop monitoring
    /// </summary>
    [RelayCommand]
    private void StopMonitoring()
    {
        AutoRefresh = false;
        MessageBox.Show("Monitoring stopped", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Update timer state based on settings
    /// </summary>
    private void UpdateTimerState()
    {
        if (AutoRefresh && RefreshInterval > 0)
        {
            _refreshTimer.Interval = RefreshInterval * 1000;
            _refreshTimer.Start();
        }
        else
        {
            _refreshTimer.Stop();
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}

/// <summary>
/// Active strategy view model
/// </summary>
public partial class ActiveStrategyViewModel : ViewModelBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalPnL { get; set; }
    public DateTime ActivatedAt { get; set; }
    public DateTime? LastSignalAt { get; set; }

    public string LastSignalDisplay => LastSignalAt.HasValue
        ? LastSignalAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
        : "No signals yet";

    public string RunningTime
    {
        get
        {
            var span = DateTime.Now - ActivatedAt;
            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays}d {span.Hours}h";
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours}h {span.Minutes}m";
            return $"{(int)span.TotalMinutes}m";
        }
    }
}
