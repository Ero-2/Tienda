using Tienda.ViewModels;

namespace Tienda.Views;

public partial class OrderHistoryPage : ContentPage
{
    public OrderHistoryPage(OrderHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is OrderHistoryViewModel vm)
            await vm.LoadOrdersCommand.ExecuteAsync(null);
    }

    private async void OnBackTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");
}