using AlgoTrading.Application.Services.Strategy;
using EntityStrategy = AlgoTrading.Core.Entities.Strategy;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace AlgoTrading.Desktop.ViewModels.Strategy;

/// <summary>
/// ViewModel for Strategy Editor View
/// Create and edit trading strategies
/// </summary>
public partial class StrategyEditorViewModel : ViewModelBase
{
    private readonly IStrategyService _strategyService;
    private readonly ILogger<StrategyEditorViewModel> _logger;

    private Guid? _strategyId;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private EntityStrategy.StrategyType _selectedStrategyType = EntityStrategy.StrategyType.Trend;
    public EntityStrategy.StrategyType SelectedStrategyType
    {
        get => _selectedStrategyType;
        set => SetProperty(ref _selectedStrategyType, value);
    }

    public ObservableCollection<EntityStrategy.StrategyType> StrategyTypes { get; }

    private decimal _maxPositionSize = 10m;
    public decimal MaxPositionSize
    {
        get => _maxPositionSize;
        set => SetProperty(ref _maxPositionSize, value);
    }

    private decimal _stopLoss = 3m;
    public decimal StopLoss
    {
        get => _stopLoss;
        set => SetProperty(ref _stopLoss, value);
    }

    private decimal _takeProfit = 5m;
    public decimal TakeProfit
    {
        get => _takeProfit;
        set => SetProperty(ref _takeProfit, value);
    }

    private string _targetStocks = string.Empty;
    public string TargetStocks
    {
        get => _targetStocks;
        set => SetProperty(ref _targetStocks, value);
    }

    public ObservableCollection<StrategyRuleViewModel> Rules { get; }

    private StrategyRuleViewModel? _selectedRule;
    public StrategyRuleViewModel? SelectedRule
    {
        get => _selectedRule;
        set
        {
            SetProperty(ref _selectedRule, value);
            OnPropertyChanged(nameof(IsRuleSelected));
        }
    }

    public bool IsRuleSelected => SelectedRule != null;

    public StrategyEditorViewModel(
        IStrategyService strategyService,
        ILogger<StrategyEditorViewModel> logger)
    {
        _strategyService = strategyService ?? throw new ArgumentNullException(nameof(strategyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        StrategyTypes = new ObservableCollection<EntityStrategy.StrategyType>(Enum.GetValues<EntityStrategy.StrategyType>());
        Rules = new ObservableCollection<StrategyRuleViewModel>();
    }

    /// <summary>
    /// Load strategy for editing
    /// </summary>
    public async Task LoadStrategyAsync(Guid strategyId)
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading strategy...";

            _strategyId = strategyId;

            var result = await _strategyService.GetStrategyWithRulesAsync(strategyId);

            if (result.IsError)
            {
                _logger.LogError("Failed to load strategy: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to load strategy: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var strategy = result.Value;
            Name = strategy.Name;
            Description = strategy.Description;
            SelectedStrategyType = (EntityStrategy.StrategyType)strategy.Type;
            MaxPositionSize = strategy.MaxPositionSizePercent;
            StopLoss = strategy.StopLossPercent;
            TakeProfit = strategy.TakeProfitPercent;
            TargetStocks = string.Join(", ", strategy.TargetStocks.Select(s => s.Value));

            Rules.Clear();
            foreach (var rule in strategy.Rules)
            {
                Rules.Add(new StrategyRuleViewModel
                {
                    Id = rule.Id.Value,
                    Name = rule.Name,
                    Description = rule.Description,
                    Type = rule.Type.ToString(),
                    Condition = rule.Condition,
                    Priority = rule.Priority,
                    IsEnabled = rule.IsEnabled
                });
            }

            _logger.LogInformation("Strategy loaded: {Name}", Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading strategy");
            MessageBox.Show($"Error loading strategy: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Save strategy
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Strategy name is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            BusyMessage = "Saving strategy...";

            if (_strategyId == null)
            {
                // Create new strategy
                var createResult = await _strategyService.CreateStrategyAsync(
                    Name,
                    Description,
                    (AlgoTrading.Core.Enums.StrategyType)SelectedStrategyType,
                    Environment.UserName);

                if (createResult.IsError)
                {
                    _logger.LogError("Failed to create strategy: {Error}", createResult.FirstError.Description);
                    MessageBox.Show($"Failed to create strategy: {createResult.FirstError.Description}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _strategyId = createResult.Value.Id.Value;
                _logger.LogInformation("Strategy created: {Name}", Name);
            }

            // Update risk parameters
            var riskResult = await _strategyService.SetRiskParametersAsync(
                _strategyId.Value,
                MaxPositionSize,
                StopLoss,
                TakeProfit);

            if (riskResult.IsError)
            {
                _logger.LogWarning("Failed to update risk parameters: {Error}", riskResult.FirstError.Description);
            }

            // Update target stocks
            if (!string.IsNullOrWhiteSpace(TargetStocks))
            {
                var stockCodes = TargetStocks.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                var stocksResult = await _strategyService.SetTargetStocksAsync(
                    _strategyId.Value,
                    stockCodes);

                if (stocksResult.IsError)
                {
                    _logger.LogWarning("Failed to update target stocks: {Error}", stocksResult.FirstError.Description);
                }
            }

            MessageBox.Show("Strategy saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            _logger.LogInformation("Strategy saved: {Name}", Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving strategy");
            MessageBox.Show($"Error saving strategy: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Add new rule
    /// </summary>
    [RelayCommand]
    private void AddRule()
    {
        var newRule = new StrategyRuleViewModel
        {
            Id = Guid.NewGuid(),
            Name = "New Rule",
            Description = "Enter rule description",
            Type = EntityStrategy.RuleType.EntryLong.ToString(),
            Condition = "// Enter condition expression",
            Priority = Rules.Count + 1,
            IsEnabled = true
        };

        Rules.Add(newRule);
        SelectedRule = newRule;
    }

    /// <summary>
    /// Remove selected rule
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsRuleSelected))]
    private async Task RemoveRuleAsync()
    {
        if (SelectedRule == null || _strategyId == null) return;

        try
        {
            var result = await _strategyService.RemoveRuleAsync(_strategyId.Value, SelectedRule.Id);

            if (result.IsError)
            {
                _logger.LogError("Failed to remove rule: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to remove rule: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Rules.Remove(SelectedRule);
            SelectedRule = null;

            _logger.LogInformation("Rule removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing rule");
            MessageBox.Show($"Error removing rule: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Save rule
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsRuleSelected))]
    private async Task SaveRuleAsync()
    {
        if (SelectedRule == null || _strategyId == null) return;

        if (string.IsNullOrWhiteSpace(SelectedRule.Name))
        {
            MessageBox.Show("Rule name is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            BusyMessage = "Saving rule...";

            var ruleType = Enum.Parse<AlgoTrading.Core.Enums.RuleType>(SelectedRule.Type);

            var result = await _strategyService.AddRuleAsync(
                _strategyId.Value,
                SelectedRule.Name,
                SelectedRule.Description,
                ruleType,
                SelectedRule.Condition,
                SelectedRule.Priority);

            if (result.IsError)
            {
                _logger.LogError("Failed to save rule: {Error}", result.FirstError.Description);
                MessageBox.Show($"Failed to save rule: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Rule saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            _logger.LogInformation("Rule saved: {Name}", SelectedRule.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving rule");
            MessageBox.Show($"Error saving rule: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Cancel editing
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // TODO: Navigate back to StrategyListView
        MessageBox.Show("Cancel - Navigate back to List", "Info");
    }
}

/// <summary>
/// ViewModel for strategy rule
/// </summary>
public partial class StrategyRuleViewModel : ViewModelBase
{
    public Guid Id { get; set; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _type = string.Empty;
    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private string _condition = string.Empty;
    public string Condition
    {
        get => _condition;
        set => SetProperty(ref _condition, value);
    }

    private int _priority;
    public int Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
}
