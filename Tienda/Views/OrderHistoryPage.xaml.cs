using Tienda.ViewModels;

namespace Tienda.Views;

public partial class OrderHistoryPage : ContentPage
{
    public OrderHistoryPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}