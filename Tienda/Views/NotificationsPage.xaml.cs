using Tienda.ViewModels;

namespace Tienda.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}