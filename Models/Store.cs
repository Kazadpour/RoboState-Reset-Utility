using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RoboStateResetUtility.Models;

public partial class Store : ObservableObject
{
    [ObservableProperty]
    private string _storeNumber = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isExpanded;

    public ObservableCollection<SelfCheckout> SelfCheckouts { get; } = new();

    public string DisplayName => $"Store {StoreNumber}";

    public int AccessibleCheckoutsCount => SelfCheckouts.Count(sco => sco.IsAccessible);

    partial void OnIsSelectedChanged(bool value)
    {
        // When store is selected/deselected, update all its checkouts
        foreach (var checkout in SelfCheckouts.Where(sco => sco.IsAccessible))
        {
            checkout.IsSelected = value;
        }
    }
}
