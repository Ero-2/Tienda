// Tienda/Views/ProductDetailPage.xaml.cs
using Tienda.ViewModels;

namespace Tienda.Views;

public partial class ProductDetailPage : ContentPage
{
    public ProductDetailPage(ProductDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}