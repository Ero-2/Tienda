using Tienda.ViewModels;

namespace Tienda.Views;

public partial class AddressesPage : ContentPage
{
    public AddressesPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}