using CommunityToolkit.Mvvm.ComponentModel;

namespace RoboStateResetUtility.Models;

public partial class SelfCheckout : ObservableObject
{
    [ObservableProperty]
    private string _checkoutNumber = string.Empty;

    [ObservableProperty]
    private string _networkPath = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isAccessible;

    [ObservableProperty]
    private string _status = "Pending";

    [ObservableProperty]
    private int? _currentResetValue;

    [ObservableProperty]
    private string _statusColor = "#FFFFFF";

    public string DisplayName => $"SCO {CheckoutNumber}";

    public string FullPath => $"{NetworkPath}\\Robot\\Data\\vpos_state.cfg";
}
