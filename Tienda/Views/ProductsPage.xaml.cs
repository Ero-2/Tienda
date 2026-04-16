using Tienda.ViewModels;

namespace Tienda.Views;

public partial class ProductsPage : ContentPage
{
    public ProductsPage(ProductsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ejecuta la carga de datos cada vez que se abre la vista
        if (BindingContext is ProductsViewModel viewModel)
        {
            viewModel.LoadProductsCommand.Execute(null);
        }
    }
}