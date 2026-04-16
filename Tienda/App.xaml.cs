// Tienda/App.xaml.cs
using Microsoft.Extensions.DependencyInjection;

namespace Tienda;

public partial class App : Application
{
    public App()
    {
         InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Obtener el ServiceProvider de la aplicación
        var serviceProvider = (Application.Current as App)?.Handler.MauiContext?.Services;

        // Crear AppShell pasándole el ServiceProvider
        return new Window(new AppShell(serviceProvider));
    }
}