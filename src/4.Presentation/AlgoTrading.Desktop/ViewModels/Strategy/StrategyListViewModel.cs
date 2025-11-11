using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.Strategy;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace AlgoTrading.Desktop.ViewModels.Strategy;

/// <summary>
/// ViewModel for Strategy List View
/// Displays and manages trading strategies
/// </summary>
public partial class StrategyListViewModel : ViewModelBase
{
    private readonly IStrategyService _strategyService;
    private readonly ILogger<StrategyListViewModel> _logger;

    public ObservableCollection<StrategyItemViewModel> Strategies { get; }

    private StrategyItemViewModel? _selectedStrategy;
    public StrategyItemViewModel? SelectedStrategy
    {
        get => _selectedStrategy;
        set
        {
            SetProperty(ref _selectedStrategy, value);
            OnPropertyChanged(nameof(IsStrategySelected));
        }
    }

    public bool IsStrategySelected => SelectedStrategy != null;

    public StrategyListViewModel(
        IStrategyService strategyService,
        ILogger<StrategyListViewModel> logger)
    {
        _strategyService = strategyService ?? throw new ArgumentNullException(nameof(strategyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Strategies = new ObservableCollection<StrategyItemViewModel>();
    }

    /// <summary>
    /// Load all strategies from the database
    /// </summary>
    [RelayCommand]
    private async Task LoadStrategiesAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading strategies...";

            var result = await _strategyService.GetAllStrategiesAsync();

            if (result.IsError)
            {
                _logger.LogError("Failed to load strategies: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to load strategies: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Strategies.Clear();
            foreach (var strategy in result.Value)
            {
                Strategies.Add(new StrategyItemViewModel(strategy));
            }

            _logger.LogInformation("Loaded {Count} strategies", Strategies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading strategies");
            MessageBox.Show($"Error loading strategies: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Create new strategy
    /// </summary>
    [RelayCommand]
    private void CreateStrategy()
    {
        // TODO: Navigate to StrategyEditorView
        MessageBox.Show("Create Strategy - Navigate to Editor", "Info");
    }

    /// <summary>
    /// Edit selected strategy
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsStrategySelected))]
    private void EditStrategy()
    {
        if (SelectedStrategy == null) return;

        // TODO: Navigate to StrategyEditorView with strategy ID
        MessageBox.Show($"Edit Strategy: {SelectedStrategy.Name}", "Info");
    }

    /// <summary>
    /// Delete selected strategy
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsStrategySelected))]
    private async Task DeleteStrategyAsync()
    {
        if (SelectedStrategy == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete strategy '{SelectedStrategy.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Deleting strategy...";

            var deleteResult = await _strategyService.DeleteStrategyAsync(SelectedStrategy.Id);

            if (deleteResult.IsError)
            {
                _logger.LogError("Failed to delete strategy: {Error}", deleteResult.FirstError.Description);
                MessageBox.Show($"Failed to delete strategy: {deleteResult.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Strategies.Remove(SelectedStrategy);
            SelectedStrategy = null;

            _logger.LogInformation("Strategy deleted successfully");
            MessageBox.Show("Strategy deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting strategy");
            MessageBox.Show($"Error deleting strategy: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Activate selected strategy
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsStrategySelected))]
    private async Task ActivateStrategyAsync()
    {
        if (SelectedStrategy == null) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Activating strategy...";

            var result = await _strategyService.ActivateStrategyAsync(SelectedStrategy.Id);

            if (result.IsError)
            {
                _logger.LogError("Failed to activate strategy: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to activate strategy: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedStrategy.Status = "Active";
            _logger.LogInformation("Strategy activated successfully");
            MessageBox.Show("Strategy activated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating strategy");
            MessageBox.Show($"Error activating strategy: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Deactivate selected strategy
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsStrategySelected))]
    private async Task DeactivateStrategyAsync()
    {
        if (SelectedStrategy == null) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Deactivating strategy...";

            var result = await _strategyService.DeactivateStrategyAsync(SelectedStrategy.Id);

            if (result.IsError)
            {
                _logger.LogError("Failed to deactivate strategy: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to deactivate strategy: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedStrategy.Status = "Inactive";
            _logger.LogInformation("Strategy deactivated successfully");
            MessageBox.Show("Strategy deactivated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating strategy");
            MessageBox.Show($"Error deactivating strategy: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Clone selected strategy
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsStrategySelected))]
    private async Task CloneStrategyAsync()
    {
        if (SelectedStrategy == null) return;

        var newName = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter name for cloned strategy:",
            "Clone Strategy",
            $"{SelectedStrategy.Name} (Copy)");

        if (string.IsNullOrWhiteSpace(newName)) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Cloning strategy...";

            var result = await _strategyService.CloneStrategyAsync(
                SelectedStrategy.Id,
                newName,
                Environment.UserName);

            if (result.IsError)
            {
                _logger.LogError("Failed to clone strategy: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to clone strategy: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Strategies.Add(new StrategyItemViewModel(result.Value));
            _logger.LogInformation("Strategy cloned successfully");
            MessageBox.Show("Strategy cloned successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning strategy");
            MessageBox.Show($"Error cloning strategy: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Refresh strategies list
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadStrategiesAsync();
    }
}

/// <summary>
/// ViewModel for individual strategy item in the list
/// </summary>
public partial class StrategyItemViewModel : ViewModelBase
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Type { get; }

    private string _status;
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public int TotalTrades { get; }
    public decimal WinRate { get; }
    public decimal TotalPnL { get; }
    public DateTime CreatedAt { get; }

    public StrategyItemViewModel(TradingStrategy strategy)
    {
        Id = strategy.Id.Value;
        Name = strategy.Name;
        Description = strategy.Description;
        Type = strategy.Type.ToString();
        _status = strategy.Status.ToString();
        TotalTrades = strategy.TotalTrades;
        WinRate = strategy.WinRate;
        TotalPnL = strategy.TotalProfitLoss;
        CreatedAt = strategy.CreatedAt;
    }
}
