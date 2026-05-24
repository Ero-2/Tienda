// Tienda/AppShell.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using Tienda.Views;

namespace Tienda;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("ProductDetailPage", typeof(ProductDetailPage));
        Routing.RegisterRoute("CartPage", typeof(CartPage));
        Routing.RegisterRoute("AccountPage", typeof(AccountPage));
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage)); // Añade esta si tienes RegisterPage
        Routing.RegisterRoute("OrderHistoryPage", typeof(OrderHistoryPage));
        Routing.RegisterRoute("PaymentPage", typeof(PaymentPage));
        Routing.RegisterRoute("AddressesPage", typeof(AddressesPage));
        Routing.RegisterRoute("NotificationsPage", typeof(NotificationsPage));
        Routing.RegisterRoute("PromocionesPage", typeof(PromocionesPage));
        Routing.RegisterRoute("OrderTrackingPage", typeof(OrderTrackingPage));
    }
}