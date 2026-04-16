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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // ✅ Ahora accedemos a la propiedad pública IsLoggedIn
        if (!_viewModel.IsLoggedIn)
        {
            Shell.Current.GoToAsync("///LoginPage");
        }
    }
}