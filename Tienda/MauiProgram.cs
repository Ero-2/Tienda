using Microsoft.Extensions.Logging;
using Tienda.Data;
using Tienda.ViewModels;
using Tienda.Views;

namespace Tienda
{
    public static class MauiProgram
    {
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

            // 1. Registro de la Base de Datos (SQLite)
            // Usamos AddDbContext para que EF Core gestione el ciclo de vida
            builder.Services.AddDbContext<AppDbContext>();

            // 2. Registro de ViewModels
            // Singleton para mantener el estado de los productos mientras la app viva
            builder.Services.AddSingleton<ProductsViewModel>();
            // Transient para la cuenta, se crea y destruye cada vez que entras
            builder.Services.AddTransient<AccountViewModel>();

            // 3. Registro de Vistas (Pages)
            // Importante: Deben estar registradas para que el constructor reciba el ViewModel
            builder.Services.AddSingleton<ProductsPage>();
            builder.Services.AddTransient<AccountPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}