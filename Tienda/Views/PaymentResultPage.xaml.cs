using Tienda.ViewModels;

namespace Tienda.Views;

public partial class PaymentResultPage : ContentPage
{
    public PaymentResultPage(PaymentResultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
