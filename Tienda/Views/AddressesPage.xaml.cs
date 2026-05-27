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
        {
            vm.IsAddingAddress = false;
            await vm.LoadAddressesCommand.ExecuteAsync(null);
        }
    }

    private async void OnBackTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
