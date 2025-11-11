using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgoTrading.Desktop.ViewModels;

/// <summary>
/// Base class for all ViewModels
/// Provides common functionality using CommunityToolkit.Mvvm
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _busyMessage = string.Empty;

    /// <summary>
    /// Indicates if the ViewModel is currently busy
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Message to display when busy
    /// </summary>
    public string BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }
}
