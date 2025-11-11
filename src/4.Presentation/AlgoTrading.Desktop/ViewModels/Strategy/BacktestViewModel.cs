using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Enums;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;

namespace AlgoTrading.Desktop.ViewModels.Strategy;

/// <summary>
/// ViewModel for Backtest View
/// Run backtests and display results
/// </summary>
public partial class BacktestViewModel : ViewModelBase
{
    private readonly IBacktestService _backtestService;
    private readonly IStrategyService _strategyService;
    private readonly IStrategyOptimizationService _optimizationService;
    private readonly ILogger<BacktestViewModel> _logger;

    private Guid? _selectedStrategyId;
    public Guid? SelectedStrategyId
    {
        get => _selectedStrategyId;
        set
        {
            SetProperty(ref _selectedStrategyId, value);
            OnPropertyChanged(nameof(IsStrategySelected));
        }
    }

    public bool IsStrategySelected => SelectedStrategyId != null;

    public ObservableCollection<StrategyListItem> Strategies { get; }

    private DateTime _startDate = DateTime.Now.AddYears(-1);
    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    private DateTime _endDate = DateTime.Now;
    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    private decimal _initialCapital = 10000000m; // 10 million KRW
    public decimal InitialCapital
    {
        get => _initialCapital;
        set => SetProperty(ref _initialCapital, value);
    }

    // Backtest Results
    private BacktestResultViewModel? _result;
    public BacktestResultViewModel? Result
    {
        get => _result;
        set
        {
            SetProperty(ref _result, value);
            OnPropertyChanged(nameof(HasResult));
        }
    }

    public bool HasResult => Result != null;

    // Optimization Settings
    private OptimizationType _optimizationType = OptimizationType.GridSearch;
    public OptimizationType OptimizationType
    {
        get => _optimizationType;
        set => SetProperty(ref _optimizationType, value);
    }

    public ObservableCollection<OptimizationType> OptimizationTypes { get; }

    private OptimizationObjective _optimizationObjective = OptimizationObjective.MaxSharpe;
    public OptimizationObjective OptimizationObjective
    {
        get => _optimizationObjective;
        set => SetProperty(ref _optimizationObjective, value);
    }

    public ObservableCollection<OptimizationObjective> OptimizationObjectives { get; }

    public BacktestViewModel(
        IBacktestService backtestService,
        IStrategyService strategyService,
        IStrategyOptimizationService optimizationService,
        ILogger<BacktestViewModel> logger)
    {
        _backtestService = backtestService ?? throw new ArgumentNullException(nameof(backtestService));
        _strategyService = strategyService ?? throw new ArgumentNullException(nameof(strategyService));
        _optimizationService = optimizationService ?? throw new ArgumentNullException(nameof(optimizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Strategies = new ObservableCollection<StrategyListItem>();
        OptimizationTypes = new ObservableCollection<OptimizationType>(Enum.GetValues<OptimizationType>());
        OptimizationObjectives = new ObservableCollection<OptimizationObjective>(Enum.GetValues<OptimizationObjective>());
    }

    /// <summary>
    /// Load available strategies
    /// </summary>
    [RelayCommand]
    private async Task LoadStrategiesAsync()
    {
        try
        {
            var result = await _strategyService.GetAllStrategiesAsync();

            if (result.IsError)
            {
                _logger.LogError("Failed to load strategies: {Error}", result.FirstError.Description);
                return;
            }

            Strategies.Clear();
            foreach (var strategy in result.Value)
            {
                Strategies.Add(new StrategyListItem
                {
                    Id = strategy.Id.Value,
                    Name = strategy.Name
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading strategies");
        }
    }

    /// <summary>
    /// Run backtest
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsStrategySelected))]
    private async Task RunBacktestAsync()
    {
        if (!SelectedStrategyId.HasValue) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Running backtest...";

            var result = await _backtestService.RunBacktestAsync(
                SelectedStrategyId.Value,
                StartDate,
                EndDate,
                InitialCapital);

            if (result.IsError)
            {
                _logger.LogError("Backtest failed: {Error}", result.FirstError.Description);
                MessageBox.Show($"Backtest failed: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Result = new BacktestResultViewModel(result.Value);

            _logger.LogInformation("Backtest completed: Total Trades={TotalTrades}, Win Rate={WinRate}%",
                Result.TotalTrades, Result.WinRate);

            MessageBox.Show("Backtest completed successfully", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running backtest");
            MessageBox.Show($"Error running backtest: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Run optimization
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsStrategySelected))]
    private async Task RunOptimizationAsync()
    {
        if (!SelectedStrategyId.HasValue) return;

        try
        {
            IsBusy = true;
            BusyMessage = $"Running {OptimizationType} optimization...";

            // Define parameter ranges (example)
            var parameterRanges = new Dictionary<string, ParameterRange>
            {
                { "FastEMA", new ParameterRange(5, 20, 1, ParameterType.Integer) },
                { "SlowEMA", new ParameterRange(20, 50, 5, ParameterType.Integer) },
                { "RSIThreshold", new ParameterRange(30, 70, 5, ParameterType.Decimal) }
            };

            ErrorOr.ErrorOr<OptimizationResult> result = OptimizationType switch
            {
                OptimizationType.GridSearch => await _optimizationService.GridSearchAsync(
                    SelectedStrategyId.Value,
                    parameterRanges,
                    OptimizationObjective,
                    StartDate,
                    EndDate),

                OptimizationType.RandomSearch => await _optimizationService.RandomSearchAsync(
                    SelectedStrategyId.Value,
                    parameterRanges,
                    OptimizationObjective,
                    StartDate,
                    EndDate,
                    iterations: 50),

                OptimizationType.GeneticAlgorithm => await _optimizationService.GeneticAlgorithmAsync(
                    SelectedStrategyId.Value,
                    parameterRanges,
                    OptimizationObjective,
                    StartDate,
                    EndDate,
                    populationSize: 30,
                    generations: 50),

                _ => await _optimizationService.GridSearchAsync(
                    SelectedStrategyId.Value,
                    parameterRanges,
                    OptimizationObjective,
                    StartDate,
                    EndDate)
            };

            if (result.IsError)
            {
                _logger.LogError("Optimization failed: {Error}", result.FirstError.Description);
                MessageBox.Show($"Optimization failed: {result.FirstError.Description}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var optResult = result.Value;

            _logger.LogInformation(
                "Optimization completed: Type={Type}, Score={Score}, Evaluated={Count}, Duration={Duration}s",
                optResult.Type,
                optResult.Score,
                optResult.EvaluatedCombinations,
                optResult.Duration.TotalSeconds);

            var message = $"Optimization completed!\n\n" +
                         $"Type: {optResult.Type}\n" +
                         $"Objective: {optResult.Objective}\n" +
                         $"Best Score: {optResult.Score:N2}\n" +
                         $"Evaluated: {optResult.EvaluatedCombinations} combinations\n" +
                         $"Duration: {optResult.Duration.TotalSeconds:N1} seconds\n\n" +
                         $"Optimal Parameters:\n" +
                         string.Join("\n", optResult.OptimalParameters.Select(p => $"  {p.Key}: {p.Value}"));

            MessageBox.Show(message, "Optimization Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running optimization");
            MessageBox.Show($"Error running optimization: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = string.Empty;
        }
    }

    /// <summary>
    /// Export results to CSV
    /// </summary>
    [RelayCommand]
    private void ExportResults()
    {
        if (Result == null) return;

        MessageBox.Show("Export to CSV - Not implemented yet", "Info");
        // TODO: Implement CSV export
    }
}

/// <summary>
/// Strategy list item
/// </summary>
public class StrategyListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Backtest result view model
/// </summary>
public class BacktestResultViewModel
{
    public Guid BacktestId { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public decimal InitialCapital { get; }
    public decimal FinalCapital { get; }
    public decimal TotalReturn { get; }
    public decimal AnnualizedReturn { get; }
    public decimal SharpeRatio { get; }
    public decimal MaxDrawdown { get; }
    public decimal MaxDrawdownPercent { get; }
    public int TotalTrades { get; }
    public int WinningTrades { get; }
    public int LosingTrades { get; }
    public decimal WinRate { get; }
    public decimal ProfitFactor { get; }
    public decimal AverageWin { get; }
    public decimal AverageLoss { get; }
    public decimal LargestWin { get; }
    public decimal LargestLoss { get; }

    public BacktestResultViewModel(BacktestResult result)
    {
        BacktestId = result.BacktestId;
        StartDate = result.StartDate;
        EndDate = result.EndDate;
        InitialCapital = result.InitialCapital;
        FinalCapital = result.FinalCapital;
        TotalReturn = result.Metrics.TotalReturn;
        AnnualizedReturn = result.Metrics.AnnualizedReturn;
        SharpeRatio = result.Metrics.SharpeRatio;
        MaxDrawdown = result.Metrics.MaxDrawdown;
        MaxDrawdownPercent = result.Metrics.MaxDrawdownPercent;
        TotalTrades = result.Metrics.TotalTrades;
        WinningTrades = result.Metrics.WinningTrades;
        LosingTrades = result.Metrics.LosingTrades;
        WinRate = result.Metrics.WinRate;
        ProfitFactor = result.Metrics.ProfitFactor;
        AverageWin = result.Metrics.AverageWin;
        AverageLoss = result.Metrics.AverageLoss;
        LargestWin = result.Metrics.LargestWin;
        LargestLoss = result.Metrics.LargestLoss;
    }
}
