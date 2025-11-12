using AlgoTrading.Desktop.ViewModels;
using AlgoTrading.Desktop.ViewModels.Strategy;
using AlgoTrading.Desktop.Views;
using AlgoTrading.Desktop.Views.Strategy;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AlgoTrading.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Navigate to Dashboard on startup
        NavigateTo("Dashboard");
    }

    /// <summary>
    /// Handle navigation button clicks
    /// </summary>
    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton button && button.Tag is string viewName)
        {
            NavigateTo(viewName);
        }
    }

    /// <summary>
    /// Navigate to specified view
    /// </summary>
    private void NavigateTo(string viewName)
    {
        UserControl? view = null;

        switch (viewName)
        {
            case "Dashboard":
                view = new DashboardView
                {
                    DataContext = App.ServiceProvider.GetRequiredService<DashboardViewModel>()
                };
                break;

            case "Orders":
                var orderEntryView = new OrderEntryView
                {
                    DataContext = App.ServiceProvider.GetRequiredService<OrderEntryViewModel>()
                };
                // Initialize the view model
                var orderViewModel = orderEntryView.DataContext as OrderEntryViewModel;
                orderViewModel?.InitializeCommand.Execute(null);
                view = orderEntryView;
                break;

            case "Strategies":
                view = new StrategyListView
                {
                    DataContext = App.ServiceProvider.GetRequiredService<StrategyListViewModel>()
                };
                break;

            case "Backtest":
                view = new BacktestView
                {
                    DataContext = App.ServiceProvider.GetRequiredService<BacktestViewModel>()
                };
                break;

            case "Monitor":
                view = new StrategyMonitorView
                {
                    DataContext = App.ServiceProvider.GetRequiredService<StrategyMonitorViewModel>()
                };
                break;

            case "Settings":
                // TODO: Create SettingsView
                view = new UserControl
                {
                    Content = new TextBlock
                    {
                        Text = "Settings view - Coming soon!",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        FontSize = 24
                    }
                };
                break;
        }

        if (view != null)
        {
            ContentArea.Content = view;
        }
    }
}