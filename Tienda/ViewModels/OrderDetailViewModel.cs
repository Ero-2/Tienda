using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class OrderDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IOrdenService _ordenService;
    private readonly IEnvioService _envioService;

    // Orden
    [ObservableProperty] private int     ordenId;
    [ObservableProperty] private string  ordenLabel   = string.Empty;
    [ObservableProperty] private string  estado       = string.Empty;
    [ObservableProperty] private string  estadoColor  = "#CCFF00";
    [ObservableProperty] private string  fecha        = string.Empty;
    [ObservableProperty] private string  modalidad    = string.Empty;
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal descuento;
    [ObservableProperty] private decimal total;
    [ObservableProperty] private bool    tieneDescuento;
    [ObservableProperty] private bool    isLoading    = true;
    [ObservableProperty] private bool    hasError;

    [ObservableProperty]
    private ObservableCollection<ItemDetalleVm> items = new();

    // Envío
    [ObservableProperty] private bool   tieneEnvio       = false;
    [ObservableProperty] private bool   isLoadingEnvio   = false;
    [ObservableProperty] private string numRastreo       = string.Empty;
    [ObservableProperty] private string estadoEnvio      = string.Empty;
    [ObservableProperty] private string fechaEstimada    = string.Empty;
    [ObservableProperty] private string direccionEntrega = string.Empty;

    [ObservableProperty]
    private ObservableCollection<OrderStep> envioSteps = new();

    public bool IsContentVisible => !IsLoading && !HasError;
    public bool TieneDireccion   => !string.IsNullOrWhiteSpace(DireccionEntrega);

    public OrderDetailViewModel(IOrdenService ordenService, IEnvioService envioService)
    {
        _ordenService = ordenService;
        _envioService = envioService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("OrdenId", out var id))
            OrdenId = Convert.ToInt32(id);
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasError  = false;
        try
        {
            var orden = await _ordenService.ObtenerOrdenPorIdAsync(OrdenId);
            if (orden is null) { HasError = true; return; }

            OrdenLabel     = $"ORDEN #{orden.Id}";
            Fecha          = orden.CreadoEn.ToString("dd MMM yyyy — HH:mm");
            Subtotal       = orden.Subtotal;
            Descuento      = orden.DescuentoMonto;
            Total          = orden.Total;
            TieneDescuento = orden.DescuentoMonto > 0;
            Modalidad      = MapModalidad(orden.ModalidadPago, orden.MesesMsi);

            (Estado, EstadoColor) = orden.Estado switch
            {
                "confirmada" => ("CONFIRMADA", "#CCFF00"),
                "cancelada"  => ("CANCELADA",  "#FF3E00"),
                _            => ("PROCESANDO", "#FFB800")
            };

            Items = new ObservableCollection<ItemDetalleVm>(
                orden.Items.Select(i => new ItemDetalleVm
                {
                    Nombre        = i.NombreProducto,
                    Cantidad      = i.Cantidad,
                    PrecioUnit    = i.PrecioUnitario,
                    Subtotal      = i.Subtotal,
                    EsElectronico = i.EsElectronico
                }));
        }
        catch
        {
            HasError = true;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsContentVisible));
        }

        // Cargar pipeline de envío en paralelo (no bloquea la UI de la orden)
        _ = LoadEnvioAsync();
    }

    private async Task LoadEnvioAsync()
    {
        IsLoadingEnvio = true;
        try
        {
            var rastreo = await _envioService.GetRastreoAsync(OrdenId);

            if (rastreo is null)
            {
                NumRastreo    = $"ORD-{OrdenId}";
                EstadoEnvio   = "Preparando";
                FechaEstimada = "Por confirmar";
                EnvioSteps    = new ObservableCollection<OrderStep>(BuildSteps("Preparando", new()));
            }
            else
            {
                var envio = rastreo.Envio;
                NumRastreo       = string.IsNullOrEmpty(envio.NumRastreo) ? $"ORD-{OrdenId}" : envio.NumRastreo;
                EstadoEnvio      = envio.Estado;
                DireccionEntrega = envio.DireccionEntrega;
                FechaEstimada    = envio.FechaEstimada != default
                    ? envio.FechaEstimada.ToString("dd 'de' MMMM yyyy", new CultureInfo("es-MX"))
                    : "Por confirmar";
                EnvioSteps = new ObservableCollection<OrderStep>(BuildSteps(envio.Estado, rastreo.Historial));
                OnPropertyChanged(nameof(TieneDireccion));
            }

            TieneEnvio = true;
        }
        catch
        {
            TieneEnvio = false;
        }
        finally
        {
            IsLoadingEnvio = false;
        }
    }

    private static List<OrderStep> BuildSteps(string estado, List<HistorialEstadoEnvio> historial)
    {
        var stateOrder = new[] { "Preparando", "En camino", "Entregado" };
        int activeIdx  = Array.FindIndex(stateOrder,
            s => string.Equals(s, estado, StringComparison.OrdinalIgnoreCase));

        return new List<OrderStep>
        {
            new() { Title    = "Orden confirmada",
                    Subtitle = GetSubtitle(historial, null, "Pago aprobado"),
                    Status   = StepStatus.Done },
            new() { Title    = "Preparando pedido",
                    Subtitle = GetSubtitle(historial, "Preparando", "En preparación"),
                    Status   = StepStatusFor(0, activeIdx) },
            new() { Title    = "En camino",
                    Subtitle = GetSubtitle(historial, "En camino", "En tránsito"),
                    Status   = StepStatusFor(1, activeIdx) },
            new() { Title    = "Entregado",
                    Subtitle = GetSubtitle(historial, "Entregado", "Pendiente"),
                    Status   = StepStatusFor(2, activeIdx) },
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
        stepIdx == activeIdx                 ? StepStatus.Active   : StepStatus.Done;

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    private static string MapModalidad(string raw, int meses) => raw?.ToLower() switch
    {
        "contado"                              => "CONTADO",
        var m when m?.Contains("msi") == true => $"{meses} MSI",
        _                                      => raw?.ToUpper() ?? "CONTADO"
    };
}

public class ItemDetalleVm
{
    public string  Nombre        { get; set; } = string.Empty;
    public int     Cantidad      { get; set; }
    public decimal PrecioUnit    { get; set; }
    public decimal Subtotal      { get; set; }
    public bool    EsElectronico { get; set; }
}
