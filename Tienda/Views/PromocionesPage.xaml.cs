using Tienda.ViewModels;

namespace Tienda.Views;

public partial class PromocionesPage : ContentPage
{
    public PromocionesPage(PromocionesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnBackTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
