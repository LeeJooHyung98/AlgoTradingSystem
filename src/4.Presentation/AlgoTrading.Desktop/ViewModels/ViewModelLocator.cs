using AlgoTrading.Desktop.ViewModels.Strategy;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoTrading.Desktop.ViewModels;

/// <summary>
/// ViewModelLocator for XAML data binding
/// Provides access to ViewModels through dependency injection
/// </summary>
public class ViewModelLocator
{
    /// <summary>
    /// Gets StrategyListViewModel instance from DI container
    /// </summary>
    public StrategyListViewModel StrategyListViewModel
        => App.ServiceProvider.GetRequiredService<StrategyListViewModel>();

    /// <summary>
    /// Gets StrategyEditorViewModel instance from DI container
    /// </summary>
    public StrategyEditorViewModel StrategyEditorViewModel
        => App.ServiceProvider.GetRequiredService<StrategyEditorViewModel>();

    /// <summary>
    /// Gets BacktestViewModel instance from DI container
    /// </summary>
    public BacktestViewModel BacktestViewModel
        => App.ServiceProvider.GetRequiredService<BacktestViewModel>();

    /// <summary>
    /// Gets StrategyMonitorViewModel instance from DI container
    /// </summary>
    public StrategyMonitorViewModel StrategyMonitorViewModel
        => App.ServiceProvider.GetRequiredService<StrategyMonitorViewModel>();

    /// <summary>
    /// Cleanup all ViewModels
    /// </summary>
    public static void Cleanup()
    {
        // Cleanup code if needed
    }
}
