// ViewModels/AddressesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Tienda.ViewModels;

public partial class AddressesViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> addresses = new()
    {
        "Calle Principal #123, Col. Centro",
        "Av. Reforma #456, Col. Norte"
    };
}