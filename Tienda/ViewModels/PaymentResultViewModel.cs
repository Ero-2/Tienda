using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tienda.ViewModels;

[QueryProperty(nameof(Estado),            "Estado")]
[QueryProperty(nameof(Mensaje),           "Mensaje")]
[QueryProperty(nameof(TransaccionId),     "TransaccionId")]
[QueryProperty(nameof(Monto),             "Monto")]
[QueryProperty(nameof(MarcaTarjeta),      "MarcaTarjeta")]
[QueryProperty(nameof(TarjetaEnmascarada),"TarjetaEnmascarada")]
[QueryProperty(nameof(OrdenId),           "OrdenId")]
public partial class PaymentResultViewModel : ObservableObject
{
    [ObservableProperty] private string  estado             = string.Empty;
    [ObservableProperty] private string  mensaje            = string.Empty;
    [ObservableProperty] private string? transaccionId;
    [ObservableProperty] private decimal monto;
    [ObservableProperty] private string? marcaTarjeta;
    [ObservableProperty] private string? tarjetaEnmascarada;
    [ObservableProperty] private int     ordenId;

    // ── Propiedades derivadas del estado ────────────────────────────────────

    public bool EsAprobado  => Estado == "aprobado";
    public bool EsRechazado => Estado == "rechazado";
    public bool EsCancelado => Estado == "cancelado";

    public string Icono => Estado switch
    {
        "aprobado"  => "✓",
        "rechazado" => "✕",
        "cancelado" => "⊘",
        _           => "?"
    };

    public string Titulo => Estado switch
    {
        "aprobado"  => "¡PAGO EXITOSO!",
        "rechazado" => "PAGO RECHAZADO",
        "cancelado" => "PAGO CANCELADO",
        _           => "ESTADO DESCONOCIDO"
    };

    public string Subtitulo => Estado switch
    {
        "aprobado"  => "Tu compra fue procesada correctamente.",
        "rechazado" => "Tu banco no autorizó el cargo.",
        "cancelado" => "El pago fue cancelado durante el proceso.",
        _           => string.Empty
    };

    public Color ColorAccent => Estado switch
    {
        "aprobado"  => Color.FromArgb("#CCFF00"),
        "rechazado" => Color.FromArgb("#FF3E00"),
        "cancelado" => Color.FromArgb("#FF9500"),
        _           => Color.FromArgb("#888888")
    };

    public Color ColorFondo => Estado switch
    {
        "aprobado"  => Color.FromArgb("#0D1A00"),
        "rechazado" => Color.FromArgb("#1A0000"),
        "cancelado" => Color.FromArgb("#1A0D00"),
        _           => Color.FromArgb("#141414")
    };

    public Color ColorBorde => Estado switch
    {
        "aprobado"  => Color.FromArgb("#CCFF0044"),
        "rechazado" => Color.FromArgb("#FF3E0044"),
        "cancelado" => Color.FromArgb("#FF950044"),
        _           => Color.FromArgb("#33333344")
    };

    public string TextoBotonPrimario => EsAprobado
        ? "VER MI PEDIDO"
        : "INTENTAR DE NUEVO";

    public Color ColorBotonPrimario => Estado switch
    {
        "aprobado"  => Color.FromArgb("#CCFF00"),
        "rechazado" => Color.FromArgb("#FF3E00"),
        "cancelado" => Color.FromArgb("#FF9500"),
        _           => Color.FromArgb("#888888")
    };

    public Color ColorTextoPrimario => EsAprobado
        ? Color.FromArgb("#0D0D0D")
        : Color.FromArgb("#FFFFFF");

    public string MontoFormateado   => $"${Monto:F2} MXN";
    public bool   TieneTransaccion  => !string.IsNullOrEmpty(TransaccionId);
    public bool   TieneTarjeta      => !string.IsNullOrEmpty(TarjetaEnmascarada);

    partial void OnEstadoChanged(string value)
    {
        OnPropertyChanged(nameof(EsAprobado));
        OnPropertyChanged(nameof(EsRechazado));
        OnPropertyChanged(nameof(EsCancelado));
        OnPropertyChanged(nameof(Icono));
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(Subtitulo));
        OnPropertyChanged(nameof(ColorAccent));
        OnPropertyChanged(nameof(ColorFondo));
        OnPropertyChanged(nameof(ColorBorde));
        OnPropertyChanged(nameof(TextoBotonPrimario));
        OnPropertyChanged(nameof(ColorBotonPrimario));
        OnPropertyChanged(nameof(ColorTextoPrimario));
    }

    partial void OnMontoChanged(decimal value) =>
        OnPropertyChanged(nameof(MontoFormateado));

    partial void OnTransaccionIdChanged(string? value) =>
        OnPropertyChanged(nameof(TieneTransaccion));

    partial void OnTarjetaEnmascaradaChanged(string? value) =>
        OnPropertyChanged(nameof(TieneTarjeta));

    // ── Comandos ────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task AccionPrimariaAsync()
    {
        if (EsAprobado)
        {
            await Shell.Current.GoToAsync("OrderTrackingPage",
                new Dictionary<string, object> { ["OrderId"] = OrdenId.ToString() });
        }
        else
        {
            // Volver a PaymentPage (dos pasos atrás: esta page + la anterior)
            await Shell.Current.GoToAsync("../..");
        }
    }

    [RelayCommand]
    private async Task IrAlInicioAsync()
    {
        await Shell.Current.GoToAsync("//ProductsPage");
    }
}
