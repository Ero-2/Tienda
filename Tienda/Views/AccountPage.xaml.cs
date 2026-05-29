// Tienda/Views/AccountPage.xaml.cs
using Tienda.ViewModels;

namespace Tienda.Views;

public partial class AccountPage : ContentPage
{
    private readonly AccountViewModel _viewModel;

    public AccountPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_viewModel.IsLoggedIn)
        {
            await Shell.Current.GoToAsync("///LoginPage");
            return;
        }

        // Refresca nombre/iniciales del usuario actual (el VM es singleton y podría
        // tener datos de una sesión anterior).
        _viewModel.LoadUserData();
        await _viewModel.LoadStatsCommand.ExecuteAsync(null);
    }
}
