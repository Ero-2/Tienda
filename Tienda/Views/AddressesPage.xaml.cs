using Tienda.ViewModels;

namespace Tienda.Views;

public partial class AddressesPage : ContentPage
{
    public AddressesPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AccountViewModel vm)
            await vm.LoadAddressesCommand.ExecuteAsync(null);
    }
}
