using Tienda.ViewModels;

namespace Tienda.Views;

public partial class CuotasPage : ContentPage
{
    public CuotasPage(CuotasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
