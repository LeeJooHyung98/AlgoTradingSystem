using AlgoTrading.Desktop.ViewModels;
using System.Windows.Controls;

namespace AlgoTrading.Desktop.Views;

/// <summary>
/// Interaction logic for DashboardView.xaml
/// </summary>
public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();

        // Initialize ViewModel when DataContext is set
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is DashboardViewModel viewModel)
        {
            await viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}
