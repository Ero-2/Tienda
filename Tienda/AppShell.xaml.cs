// Tienda/AppShell.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using Tienda.Services;
using Tienda.Views;

namespace Tienda;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        // LoginPage, ProductsPage, CartPage y AccountPage son ShellContent en AppShell.xaml;
        // no se registran aquí para evitar rutas duplicadas. Se navegan con //LoginPage, //CartPage, etc.
        Routing.RegisterRoute("ProductDetailPage", typeof(ProductDetailPage));
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("OrderHistoryPage", typeof(OrderHistoryPage));
        Routing.RegisterRoute("PaymentPage", typeof(PaymentPage));
        Routing.RegisterRoute("AddressesPage", typeof(AddressesPage));
        Routing.RegisterRoute("NotificationsPage", typeof(NotificationsPage));
        Routing.RegisterRoute("PromocionesPage", typeof(PromocionesPage));
        Routing.RegisterRoute("OrderTrackingPage", typeof(OrderTrackingPage));
        Routing.RegisterRoute("CuotasPage",       typeof(CuotasPage));
        Routing.RegisterRoute("OrderDetailPage",    typeof(OrderDetailPage));
        Routing.RegisterRoute("PaymentResultPage", typeof(PaymentResultPage));

        // Gate de arranque: la app abre en LoginPage (primer ShellContent). Si ya hay
        // sesión válida, salta a las tabs; si no, se queda en Login (sin perfil fantasma).
        var auth = services?.GetService<IAuthService>();
        if (auth?.IsLoggedIn == true)
            Dispatcher.Dispatch(async () => await GoToAsync("//ProductsPage"));
    }
}