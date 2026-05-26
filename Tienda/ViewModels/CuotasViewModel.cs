using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tienda.Services;

namespace Tienda.ViewModels;

[QueryProperty(nameof(PlanId), "PlanId")]
public partial class CuotasViewModel : ObservableObject
{
    private readonly IPagosService  _pagosService;
    private readonly IAuthService   _authService;

    [ObservableProperty] private int     planId;
    [ObservableProperty] private string  numeroTarjeta  = string.Empty;
    [ObservableProperty] private string  titular        = string.Empty;
    [ObservableProperty] private string  mesExpiracion  = string.Empty;
    [ObservableProperty] private string  anioExpiracion = string.Empty;
    [ObservableProperty] private string  cvv            = string.Empty;
    [ObservableProperty] private bool    isLoading      = false;
    [ObservableProperty] private string  errorMessage   = string.Empty;
    [ObservableProperty] private string  successMessage = string.Empty;

    public CuotasViewModel(IPagosService pagosService, IAuthService authService)
    {
        _pagosService = pagosService;
        _authService  = authService;
    }

    [RelayCommand]
    private async Task PagarCuotaAsync()
    {
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;

        if (!ValidarCampos()) return;

        IsLoading = true;
        try
        {
            var userId  = _authService.GetUserId();
            var tarjeta = new TarjetaData
            {
                NumeroTarjeta = NumeroTarjeta.Replace(" ", ""),
                NombreTitular = Titular,
                Mes           = int.Parse(MesExpiracion),
                Anio          = int.Parse(AnioExpiracion),
                Cvv           = Cvv
            };

            var (ok, error) = await _pagosService.PagarCuotaAsync(PlanId, userId, tarjeta);

            if (ok)
            {
                SuccessMessage = "¡Cuota pagada exitosamente!";
                await Task.Delay(1500);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = error ?? "Error al procesar el pago.";
            }
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
