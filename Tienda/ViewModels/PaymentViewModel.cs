using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tienda.Services;

namespace Tienda.ViewModels;

[QueryProperty(nameof(ModalidadPago),    "ModalidadPago")]
[QueryProperty(nameof(Total),           "Total")]
[QueryProperty(nameof(Subtotal),        "Subtotal")]
[QueryProperty(nameof(Descuento),       "Descuento")]
[QueryProperty(nameof(CostoEnvio),      "CostoEnvio")]
[QueryProperty(nameof(DireccionEntrega),"DireccionEntrega")]
public partial class PaymentViewModel : ObservableObject
{
    private readonly IOrdenService   _ordenService;
    private readonly IPagosService   _pagosService;
    private readonly ICartService    _cartService;
    private readonly IClienteService _clienteService;
    private readonly IAuthService    _authService;

    [ObservableProperty] private string  modalidadPago = string.Empty;
    [ObservableProperty] private decimal total;
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal descuento;
    [ObservableProperty] private decimal costoEnvio;

    [ObservableProperty] private string  direccionEntrega = string.Empty;

    [ObservableProperty] private string  numeroTarjeta  = string.Empty;
    [ObservableProperty] private string  titular        = string.Empty;
    [ObservableProperty] private string  email          = string.Empty;
    [ObservableProperty] private string  mesExpiracion  = string.Empty;
    [ObservableProperty] private string  anioExpiracion = string.Empty;
    [ObservableProperty] private string  cvv            = string.Empty;

    [ObservableProperty] private bool    isLoading      = false;
    [ObservableProperty] private string  errorMessage   = string.Empty;

    // Crédito
    [ObservableProperty] private decimal creditoDisponible;
    [ObservableProperty] private bool    creditoCargado;

    public bool EsPagoCredito  => ModalidadPago == "Crédito";
    public bool EsPagoTarjeta  => !EsPagoCredito;

    // Meses del plan MSI seleccionado (0 = contado/crédito).
    public int MesesPlan => ModalidadPago switch { "3 MSI" => 3, "6 MSI" => 6, "9 MSI" => 9, _ => 0 };

    // MSI = "sin intereses": el TOTAL no cambia, solo se reparte.
    public bool EsMsi => MesesPlan > 0;

    // Lo que se cobra HOY: con MSI es la primera mensualidad; si no, el total completo.
    public decimal MontoHoy => EsMsi && Total > 0 ? Math.Round(Total / MesesPlan, 2) : Total;

    public string MsiLabel =>
        (!EsMsi || Total <= 0)
            ? string.Empty
            : $"{MesesPlan} mensualidades de ${Total / MesesPlan:F2} · sin intereses";

    public List<string> PaymentOptions
    {
        get
        {
            var opts = new List<string> { "Contado" };
            if (Total >= 50m)  opts.Add("3 MSI");
            if (Total >= 150m) opts.Add("6 MSI");
            if (Total >= 300m) opts.Add("9 MSI");
            opts.Add("Crédito");
            return opts;
        }
    }

    partial void OnModalidadPagoChanged(string value)
    {
        OnPropertyChanged(nameof(MsiLabel));
        OnPropertyChanged(nameof(MesesPlan));
        OnPropertyChanged(nameof(EsMsi));
        OnPropertyChanged(nameof(MontoHoy));
        OnPropertyChanged(nameof(EsPagoCredito));
        OnPropertyChanged(nameof(EsPagoTarjeta));
        if (value == "Crédito") _ = CargarCreditoAsync();
    }
    partial void OnTotalChanged(decimal value)
    {
        OnPropertyChanged(nameof(MsiLabel));
        OnPropertyChanged(nameof(EsMsi));
        OnPropertyChanged(nameof(MontoHoy));
        OnPropertyChanged(nameof(PaymentOptions));
        // Resetear si la opción ya no aplica con el nuevo total
        if (ModalidadPago != "Contado" && ModalidadPago != "Crédito" &&
            !PaymentOptions.Contains(ModalidadPago))
        {
            ModalidadPago = "Contado";
        }
        else
        {
            // CollectionView pierde la selección cuando ItemsSource cambia.
            // Toggle ModalidadPago para forzar que el binding re-aplique el SelectedItem.
            var saved = ModalidadPago;
            SetProperty(ref modalidadPago, string.Empty, nameof(ModalidadPago));
            SetProperty(ref modalidadPago, saved, nameof(ModalidadPago));
        }
    }

    public PaymentViewModel(
        IOrdenService   ordenService,
        IPagosService   pagosService,
        ICartService    cartService,
        IClienteService clienteService,
        IAuthService    authService)
    {
        _ordenService   = ordenService;
        _pagosService   = pagosService;
        _cartService    = cartService;
        _clienteService = clienteService;
        _authService    = authService;
    }

    private async Task CargarCreditoAsync()
    {
        var id = _authService.GetUserId();
        var credito = await _clienteService.GetCreditoAsync(id);
        if (credito is not null)
        {
            CreditoDisponible = credito.CreditoDisponible;
            CreditoCargado    = true;
        }
    }

    [RelayCommand]
    private async Task PagarAsync()
    {
        ErrorMessage = string.Empty;

        if (EsPagoCredito)
        {
            await PagarConCreditoAsync();
            return;
        }

        if (!ValidarCampos()) return;

        IsLoading = true;
        try
        {
            var items  = _cartService.GetItems();
            var orden  = await _ordenService.CrearOrdenAsync(items, ModalidadPago, DireccionEntrega);

            if (orden is null)
            {
                ErrorMessage = "No se pudo crear la orden. Intenta de nuevo.";
                return;
            }

            var checkout = await _pagosService.ObtenerCheckoutAsync(orden.Id);
            if (checkout is null)
            {
                ErrorMessage = "No se pudo obtener el checkout. Intenta de nuevo.";
                return;
            }

            var tarjeta = new TarjetaData
            {
                NumeroTarjeta = NumeroTarjeta.Replace(" ", ""),
                NombreTitular = Titular,
                Email         = Email,
                Mes           = int.Parse(MesExpiracion),
                Anio          = int.Parse(AnioExpiracion),
                Cvv           = Cvv
            };

            var resultado = await _pagosService.ConfirmarPagoAsync(checkout.TokenCheckout, tarjeta);
            if (resultado is null) { ErrorMessage = "Error al procesar el pago."; return; }

            if (resultado.Estado == "aprobado")
                _cartService.Clear();

            await Shell.Current.GoToAsync("PaymentResultPage",
                new Dictionary<string, object>
                {
                    ["Estado"]             = resultado.Estado,
                    ["Mensaje"]            = resultado.Mensaje,
                    ["TransaccionId"]      = resultado.TransaccionId      ?? string.Empty,
                    ["Monto"]              = (object)checkout.Monto,
                    ["MarcaTarjeta"]       = resultado.MarcaTarjeta       ?? string.Empty,
                    ["TarjetaEnmascarada"] = resultado.TarjetaEnmascarada ?? string.Empty,
                    ["OrdenId"]            = (object)orden.Id
                });
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsLoading = false; }
    }

    private async Task PagarConCreditoAsync()
    {
        if (CreditoDisponible < Total)
        {
            ErrorMessage = $"Crédito insuficiente. Disponible: ${CreditoDisponible:F2}";
            return;
        }

        IsLoading = true;
        try
        {
            var items = _cartService.GetItems();
            var orden = await _ordenService.CrearOrdenAsync(items, "credito", DireccionEntrega);

            if (orden is null) { ErrorMessage = "No se pudo crear la orden."; return; }

            var userId = _authService.GetUserId();
            var (ok, error) = await _clienteService.DebitarCreditoAsync(userId, Total);

            if (!ok) { ErrorMessage = error ?? "Error al debitar crédito."; return; }

            _cartService.Clear();
            await Shell.Current.GoToAsync("PaymentResultPage",
                new Dictionary<string, object>
                {
                    ["Estado"]  = "aprobado",
                    ["Mensaje"] = "Pago con crédito aplicado.",
                    ["Monto"]   = (object)Total,
                    ["OrdenId"] = (object)orden.Id
                });
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    private bool ValidarCampos()
    {
        var numero = NumeroTarjeta.Replace(" ", "");
        if (numero.Length < 15 || numero.Length > 16)
        { ErrorMessage = "Número de tarjeta inválido."; return false; }

        if (string.IsNullOrWhiteSpace(Titular))
        { ErrorMessage = "Ingresa el nombre del titular."; return false; }

        if (!int.TryParse(MesExpiracion, out var mes) || mes < 1 || mes > 12)
        { ErrorMessage = "Mes de expiración inválido (01-12)."; return false; }

        if (!int.TryParse(AnioExpiracion, out _) || AnioExpiracion.Length != 4)
        { ErrorMessage = "Año inválido (ej: 2030)."; return false; }

        if (Cvv.Length < 3 || Cvv.Length > 4)
        { ErrorMessage = "CVV inválido."; return false; }

        return true;
    }
}
