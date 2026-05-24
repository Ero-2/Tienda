using Tienda.ViewModels;

namespace Tienda.Views;

public partial class PromocionesPage : ContentPage
{
    public PromocionesPage(PromocionesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PromocionesViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    private async void OnBackTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
