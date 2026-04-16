using Tienda.ViewModels;

namespace Tienda.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}