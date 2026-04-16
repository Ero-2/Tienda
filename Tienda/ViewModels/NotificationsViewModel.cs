// ViewModels/NotificationsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tienda.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool dropsEnabled = true;

    [ObservableProperty]
    private bool ordersEnabled = true;

    [ObservableProperty]
    private bool offersEnabled = false;
}