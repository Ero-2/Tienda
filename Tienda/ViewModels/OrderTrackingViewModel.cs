using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

[QueryProperty(nameof(OrderId), "OrderId")]
public partial class OrderTrackingViewModel : ObservableObject
{
    private readonly IEnvioService _envioService;

    [ObservableProperty]
    private string orderId = string.Empty;

    [ObservableProperty]
    private string trackingNumber = "—";

    [ObservableProperty]
    private string estimatedDelivery = "—";

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private ObservableCollection<OrderStep> steps = new();

    public OrderTrackingViewModel(IEnvioService envioService)
    {
        _envioService = envioService;
    }

    partial void OnOrderIdChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        _ = LoadEnvioAsync(value);
    }

    private async Task LoadEnvioAsync(string orderIdStr)
    {
        if (!int.TryParse(orderIdStr, out var orderId)) return;

        IsLoading = true;
        var rastreo = await _envioService.GetRastreoAsync(orderId);
        IsLoading = false;

        if (rastreo is null)
        {
            TrackingNumber = $"ORD-{orderIdStr}";
            EstimatedDelivery = "Por confirmar";
            Steps = new ObservableCollection<OrderStep>
            {
                new() { Title = "Orden creada",   Subtitle = "Procesando", Status = StepStatus.Done },
                new() { Title = "En preparación", Subtitle = "Pendiente",  Status = StepStatus.Active },
                new() { Title = "En camino",      Subtitle = "Pendiente",  Status = StepStatus.Pending },
                new() { Title = "Entregado",      Subtitle = "Pendiente",  Status = StepStatus.Pending },
            };
            return;
        }

        var envio = rastreo.Envio;
        TrackingNumber = string.IsNullOrEmpty(envio.NumRastreo)
            ? $"ORD-{orderIdStr}"
            : envio.NumRastreo;

        EstimatedDelivery = envio.FechaEstimada != default
            ? envio.FechaEstimada.ToString("dddd dd 'de' MMMM", new CultureInfo("es-MX"))
            : "Por confirmar";

        Address = envio.DireccionEntrega;
        Steps = new ObservableCollection<OrderStep>(BuildSteps(envio.Estado, rastreo.Historial));
    }

    private static List<OrderStep> BuildSteps(string estado, List<HistorialEstadoEnvio> historial)
    {
        var stateOrder = new[] { "Preparando", "En camino", "Entregado" };
        int activeIdx = Array.FindIndex(stateOrder,
            s => string.Equals(s, estado, StringComparison.OrdinalIgnoreCase));

        return new List<OrderStep>
        {
            new() { Title = "Orden creada",
                    Subtitle = GetSubtitle(historial, null, "Confirmada"),
                    Status = StepStatus.Done },
            new() { Title = "Pago aprobado",
                    Subtitle = "Confirmado",
                    Status = StepStatus.Done },
            new() { Title = "Preparando pedido",
                    Subtitle = GetSubtitle(historial, "Preparando", "Pendiente"),
                    Status = StepStatusFor(0, activeIdx) },
            new() { Title = "En camino",
                    Subtitle = GetSubtitle(historial, "En camino", "En tránsito"),
                    Status = StepStatusFor(1, activeIdx) },
            new() { Title = "Entregado",
                    Subtitle = GetSubtitle(historial, "Entregado", "Pendiente"),
                    Status = StepStatusFor(2, activeIdx) },
        };
    }

    private static string GetSubtitle(List<HistorialEstadoEnvio> historial, string? estado, string fallback)
    {
        if (estado is null) return fallback;
        var entry = historial.FirstOrDefault(h =>
            string.Equals(h.Estado, estado, StringComparison.OrdinalIgnoreCase));
        return entry is not null
            ? entry.FechaRegistrado.ToString("MMM dd · HH:mm")
            : fallback;
    }

    private static StepStatus StepStatusFor(int stepIdx, int activeIdx) =>
        activeIdx < 0 || stepIdx > activeIdx ? StepStatus.Pending :
        stepIdx == activeIdx ? StepStatus.Active : StepStatus.Done;

    [RelayCommand]
    private async Task GoToHomeAsync() =>
        await Shell.Current.GoToAsync("///ProductsPage");
}
