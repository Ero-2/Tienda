using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class OrderDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IOrdenService _ordenService;

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

    public bool IsContentVisible => !IsLoading && !HasError;

    [ObservableProperty]
    private ObservableCollection<ItemDetalleVm> items = new();

    public OrderDetailViewModel(IOrdenService ordenService)
    {
        _ordenService = ordenService;
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

            OrdenLabel      = $"ORDEN #{orden.Id}";
            Fecha           = orden.CreadoEn.ToString("dd MMM yyyy — HH:mm");
            Subtotal        = orden.Subtotal;
            Descuento       = orden.DescuentoMonto;
            Total           = orden.Total;
            TieneDescuento  = orden.DescuentoMonto > 0;
            Modalidad       = MapModalidad(orden.ModalidadPago, orden.MesesMsi);

            (Estado, EstadoColor) = orden.Estado switch
            {
                "confirmada" => ("CONFIRMADA", "#CCFF00"),
                "cancelada"  => ("CANCELADA",  "#FF3E00"),
                _            => ("PROCESANDO", "#FFB800")
            };

            Items = new ObservableCollection<ItemDetalleVm>(
                orden.Items.Select(i => new ItemDetalleVm
                {
                    Nombre       = i.NombreProducto,
                    Cantidad     = i.Cantidad,
                    PrecioUnit   = i.PrecioUnitario,
                    Subtotal     = i.Subtotal,
                    EsElectronico = i.EsElectronico
                })
            );
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
    }

    [RelayCommand]
    private async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task GoToTrackingAsync() =>
        await Shell.Current.GoToAsync("OrderTrackingPage",
            new Dictionary<string, object> { ["OrderId"] = OrdenId.ToString() });

    private static string MapModalidad(string raw, int meses) => raw?.ToLower() switch
    {
        "contado"               => "CONTADO",
        var m when m?.Contains("msi") == true => $"{meses} MSI",
        _                       => raw?.ToUpper() ?? "CONTADO"
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
