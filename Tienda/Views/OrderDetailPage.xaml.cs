using Tienda.ViewModels;

namespace Tienda.Views;

public partial class OrderDetailPage : ContentPage
{
    public OrderDetailPage(OrderDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is OrderDetailViewModel vm)
            await vm.LoadCommand.ExecuteAsync(null);
    }
}
