// Tienda/ViewModels/OrderTrackingViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;

namespace Tienda.ViewModels;

[QueryProperty(nameof(OrderId), "OrderId")]
public partial class OrderTrackingViewModel : ObservableObject
{
    [ObservableProperty]
    private string orderId = string.Empty;

    [ObservableProperty]
    private string trackingNumber = string.Empty;

    [ObservableProperty]
    private string estimatedDelivery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<OrderStep> steps = new();

    partial void OnOrderIdChanged(string value)
    {
        var rand = new Random();
        TrackingNumber = $"MX·{rand.Next(1000, 9999)}·{rand.Next(1000, 9999)}";

        EstimatedDelivery = DateTime.Now.AddDays(3).ToString("dddd dd 'de' MMMM",
            new System.Globalization.CultureInfo("es-MX"));

        Steps = new ObservableCollection<OrderStep>
        {
            new() { Title = "Orden creada",       Subtitle = DateTime.Now.AddDays(-3).ToString("MMM dd · HH:mm"), Status = StepStatus.Done },
            new() { Title = "Pago aprobado",       Subtitle = DateTime.Now.AddDays(-3).ToString("MMM dd · HH:mm"), Status = StepStatus.Done },
            new() { Title = "Preparando pedido",   Subtitle = DateTime.Now.AddDays(-2).ToString("MMM dd · HH:mm"), Status = StepStatus.Done },
            new() { Title = "En camino",           Subtitle = "En tránsito",                                        Status = StepStatus.Active },
            new() { Title = "Entregado",           Subtitle = $"Est. {EstimatedDelivery}",                          Status = StepStatus.Pending },
        };
    }

    [RelayCommand]
    private async Task GoToHomeAsync() =>
        await Shell.Current.GoToAsync("///ProductsPage");
}