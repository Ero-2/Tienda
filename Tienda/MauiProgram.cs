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
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Servicios
        builder.Services.AddSingleton<AppDbContext>();
        builder.Services.AddSingleton<ICartService, CartService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddTransient<CartViewModel>();
        builder.Services.AddTransient<OrderTrackingViewModel>();
        builder.Services.AddSingleton<AccountViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();   // ✅ NUEVO
        builder.Services.AddTransient<AddressesViewModel>();      // ✅ NUEVO
        builder.Services.AddTransient<NotificationsViewModel>();  // ✅ NUEVO

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<OrderTrackingPage>();
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<OrderHistoryPage>();        // ✅ NUEVO
        builder.Services.AddTransient<AddressesPage>();           // ✅ NUEVO
        builder.Services.AddTransient<NotificationsPage>();       // ✅ NUEVO

#if DEBUG
        builder.Logging.AddDebug();
#endif

        Instance = builder.Build();
        return Instance;
    }
}