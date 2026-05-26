// Tienda/ViewModels/CartViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class CartViewModel : ObservableObject
{
    private readonly ICartService    _cartService;
    private readonly IAuthService    _authService;
    private readonly IPromocionService _promoService;
    private readonly IEnvioService   _envioService;

    [ObservableProperty] private ObservableCollection<CartItem> items = new();
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal discount;
    [ObservableProperty] private decimal costoEnvio;
    [ObservableProperty] private decimal total;
    [ObservableProperty] private string  selectedPayment = "Contado";
    [ObservableProperty] private string  discountLabel   = string.Empty;
    [ObservableProperty] private string  envioLabel      = string.Empty;
    [ObservableProperty] private bool    envioGratis;

    public int    TotalItemCount => Items.Sum(i => i.Quantity);
    public string MsiLabel
    {
        get
        {
            var meses = SelectedPayment switch { "3 MSI" => 3, "6 MSI" => 6, "12 MSI" => 12, _ => 0 };
            return (meses == 0 || Total <= 0) ? string.Empty : $"{meses} pagos de ${Total / meses:F2}/mes";
        }
    }
    public bool EsPagoCredito => SelectedPayment == "Crédito";

    public List<string> PaymentOptions { get; } =
        ["Contado", "3 MSI", "6 MSI", "12 MSI", "Crédito"];

    public CartViewModel(
        ICartService     cartService,
        IAuthService     authService,
        IPromocionService promoService,
        IEnvioService    envioService)
    {
        _cartService  = cartService;
        _authService  = authService;
        _promoService = promoService;
        _envioService = envioService;
        LoadCart();
    }

    partial void OnSelectedPaymentChanged(string value)
    {
        OnPropertyChanged(nameof(MsiLabel));
        OnPropertyChanged(nameof(EsPagoCredito));
    }

    public void LoadCart()
    {
        Items = new ObservableCollection<CartItem>(_cartService.GetItems());
        RecalculateTotals();
        _ = RefreshEnvioAsync();
        _ = ApplyPromoLabelAsync();
    }

    private void RecalculateTotals()
    {
        Subtotal = Items.Sum(i => i.Product.Price * i.Quantity);

        decimal subElec  = Items
            .Where(i => i.Product.Category?.ToLower().Contains("electr") == true)
            .Sum(i => i.Product.Price * i.Quantity);
        decimal subOtros = Subtotal - subElec;

        decimal descElec  = subElec  * 0.05m;
        decimal descOtros = subOtros >= 1000 ? subOtros * 0.10m : 0;
        Discount = Math.Round(descElec + descOtros, 2);

        DiscountLabel = (descElec > 0, descOtros > 0) switch
        {
            (true,  true)  => "5% electrónicos + 10% otros",
            (true,  false) => "Descuento 5% en electrónicos",
            (false, true)  => "Descuento 10% por compra ≥$1,000",
            _              => string.Empty
        };

        Total = Subtotal - Discount + CostoEnvio;

        OnPropertyChanged(nameof(TotalItemCount));
        OnPropertyChanged(nameof(MsiLabel));
    }

    private async Task RefreshEnvioAsync()
    {
        if (Subtotal <= 0)
        {
            CostoEnvio = 0;
            EnvioLabel = string.Empty;
            EnvioGratis = false;
            Total = Subtotal - Discount;
            return;
        }

        var costo = await _envioService.CalcularCostoAsync(Subtotal - Discount);
        CostoEnvio  = costo.CostoEnvio;
        EnvioGratis = costo.EsGratis;
        EnvioLabel  = costo.MensajePromocion;
        Total       = Subtotal - Discount + CostoEnvio;
        OnPropertyChanged(nameof(MsiLabel));
    }

    private async Task ApplyPromoLabelAsync()
    {
        if (Subtotal <= 0) return;
        decimal subElec  = Items
            .Where(i => i.Product.Category?.ToLower().Contains("electr") == true)
            .Sum(i => i.Product.Price * i.Quantity);
        var resultado = await _promoService.CalcularDescuentoAsync(subElec, Subtotal - subElec);
        if (resultado?.Motivo is { Length: > 0 } motivo && !string.IsNullOrEmpty(motivo) && motivo != "Sin descuento")
            DiscountLabel = motivo;
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        if (item.Quantity >= item.Product.Stock)
        {
            _ = Shell.Current.DisplayAlert("Sin stock", $"Solo hay {item.Product.Stock} unidades.", "OK");
            return;
        }
        item.Quantity++;
        RecalculateTotals();
        _ = RefreshEnvioAsync();
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
            item.Quantity--;
        else
        {
            _cartService.RemoveItem(item);
            Items.Remove(item);
        }
        RecalculateTotals();
        _ = RefreshEnvioAsync();
    }

    [RelayCommand]
    private async Task GoToPromocionesAsync() =>
        await Shell.Current.GoToAsync("PromocionesPage");

    [RelayCommand]
    private async Task ConfirmPurchaseAsync()
    {
        if (!Items.Any())
        {
            await Shell.Current.DisplayAlert("Carrito vacío", "Agrega productos primero.", "OK");
            return;
        }

        if (!_authService.IsLoggedIn)
        {
            bool goLogin = await Shell.Current.DisplayAlert(
                "Inicia sesión",
                "Necesitas una cuenta para confirmar tu orden.",
                "INICIAR SESIÓN", "CANCELAR");
            if (goLogin) await Shell.Current.GoToAsync("LoginPage");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "CONFIRMAR ORDEN",
            $"Total: ${Total:F2} · Pago: {SelectedPayment}",
            "CONFIRMAR", "CANCELAR");

        if (!confirm) return;

        await Shell.Current.GoToAsync("PaymentPage",
            new Dictionary<string, object>
            {
                ["ModalidadPago"] = SelectedPayment,
                ["Total"]         = Total,
                ["Subtotal"]      = Subtotal,
                ["Descuento"]     = Discount,
                ["CostoEnvio"]    = CostoEnvio
            });
    }
}
