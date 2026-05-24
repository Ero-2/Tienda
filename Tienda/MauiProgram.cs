// Tienda/MauiProgram.cs
using Microsoft.Extensions.Logging;
using Tienda.Data;
using Tienda.Services;
using Tienda.ViewModels;
using Tienda.Views;

namespace Tienda;

public static class MauiProgram
{
    public static MauiApp Instance { get; private set; } = null!;

    public static MauiApp CreateMauiApp()
    {
#if ANDROID
        const string host = "http://10.0.2.2";
#else
        const string host = "http://localhost";
#endif

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
            });

        // Infraestructura local
        builder.Services.AddSingleton<AppDbContext>();
        builder.Services.AddSingleton<ICartService, CartService>();

        // Todos los servicios van por el API Gateway (:5000)
        var gatewayUri = new Uri($"{host}:5000");

        builder.Services.AddHttpClient<IAuthService, AuthService>(c => c.BaseAddress = gatewayUri);
        builder.Services.AddHttpClient<IClienteService, ClienteService>(c => c.BaseAddress = gatewayUri);
        builder.Services.AddHttpClient<IOrdenService, OrdenService>(c => c.BaseAddress = gatewayUri);
        builder.Services.AddHttpClient<IProductoService, ProductoService>(c => c.BaseAddress = gatewayUri);
        builder.Services.AddHttpClient<IEnvioService, EnvioService>(c => c.BaseAddress = gatewayUri);
        builder.Services.AddHttpClient<IPromocionService, PromocionService>(c => c.BaseAddress = gatewayUri);

        // OpenPay (tokenización)
        builder.Services.AddSingleton<IOpenPayTokenService, OpenPayTokenService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<CartViewModel>();
        builder.Services.AddTransient<OrderTrackingViewModel>();
        builder.Services.AddTransient<PromocionesViewModel>();
        builder.Services.AddSingleton<AccountViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();
        builder.Services.AddTransient<AddressesViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<PaymentViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<OrderTrackingPage>();
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<OrderHistoryPage>();
        builder.Services.AddTransient<AddressesPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<PaymentPage>();
        builder.Services.AddTransient<PromocionesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        Instance = builder.Build();
        return Instance;
    }
}
