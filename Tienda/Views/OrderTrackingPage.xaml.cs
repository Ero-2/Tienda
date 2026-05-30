using Tienda.ViewModels;

namespace Tienda.Views;

public partial class OrderTrackingPage : ContentPage
{
    public OrderTrackingPage(OrderTrackingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
