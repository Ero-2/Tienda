using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tienda.Services;

namespace Tienda.ViewModels;

[QueryProperty(nameof(ModalidadPago), "ModalidadPago")]
[QueryProperty(nameof(Total), "Total")]
public partial class PaymentViewModel : ObservableObject
{
    private readonly IOpenPayTokenService _tokenService;
    private readonly IOrdenService        _ordenService;
    private readonly ICartService         _cartService;

    [ObservableProperty] private string  modalidadPago = string.Empty;
    [ObservableProperty] private decimal total;

    [ObservableProperty] private string numeroTarjeta   = string.Empty;
    [ObservableProperty] private string titular         = string.Empty;
    [ObservableProperty] private string mesExpiracion   = string.Empty;
    [ObservableProperty] private string anioExpiracion  = string.Empty;
    [ObservableProperty] private string cvv             = string.Empty;

    [ObservableProperty] private bool   isLoading       = false;
    [ObservableProperty] private string errorMessage    = string.Empty;

    public PaymentViewModel(
        IOpenPayTokenService tokenService,
        IOrdenService ordenService,
        ICartService cartService)
    {
        _tokenService = tokenService;
        _ordenService = ordenService;
        _cartService  = cartService;
    }

    [RelayCommand]
    private async Task PagarAsync()
    {
        ErrorMessage = string.Empty;

        if (!ValidarCampos()) return;

        IsLoading = true;
        try
        {
            // 1. Tokenizar tarjeta con OpenPay
            var token = await _tokenService.TokenizarTarjetaAsync(
                NumeroTarjeta, Titular, MesExpiracion, AnioExpiracion, Cvv);

            if (token is null)
            {
                ErrorMessage = "Datos de tarjeta inválidos. Verifica e intenta de nuevo.";
                return;
            }

            // 2. Crear orden con token
            var items = _cartService.GetItems();
            var orden = await _ordenService.CrearOrdenAsync(items, ModalidadPago, token);

            if (orden is null)
            {
                ErrorMessage = "No se pudo crear la orden. Intenta de nuevo.";
                return;
            }

            // 3. Limpiar carrito y navegar a seguimiento
            _cartService.Clear();
            await Shell.Current.GoToAsync("OrderTrackingPage",
                new Dictionary<string, object> { ["OrderId"] = $"#{orden.Id}" });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
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

        if (!int.TryParse(AnioExpiracion, out _) || AnioExpiracion.Length != 2)
        { ErrorMessage = "Año inválido (ej: 26)."; return false; }

        if (Cvv.Length < 3 || Cvv.Length > 4)
        { ErrorMessage = "CVV inválido."; return false; }

        return true;
    }
}
