// Tienda/ViewModels/CartViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class CartViewModel : ObservableObject
{
    private readonly ICartService     _cartService;
    private readonly IAuthService     _authService;
    private readonly IPromocionService _promoService;
    private readonly IEnvioService    _envioService;
    private readonly IClienteService  _clienteService;

    [ObservableProperty] private ObservableCollection<CartItem> items = new();
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal discount;
    [ObservableProperty] private decimal costoEnvio;
    [ObservableProperty] private decimal total;
    [ObservableProperty] private string  selectedPayment = "Contado";
    [ObservableProperty] private string  discountLabel   = string.Empty;
    [ObservableProperty] private string  envioLabel      = string.Empty;
    [ObservableProperty] private bool    envioGratis;
    [ObservableProperty] private bool    isBusy;

    // Dirección de entrega
    [ObservableProperty] private ObservableCollection<Address> userAddresses = new();
    [ObservableProperty] private Address? selectedAddress;
    [ObservableProperty] private bool     showAddressPicker = false;

    public bool TieneAddresses => UserAddresses.Any();
    public string SelectedAddressLabel =>
        SelectedAddress is null
            ? "Selecciona una dirección de entrega"
            : $"{SelectedAddress.Street}, {SelectedAddress.City}";

    partial void OnSelectedAddressChanged(Address? value)
    {
        ShowAddressPicker = false;
        OnPropertyChanged(nameof(SelectedAddressLabel));
    }

    public int    TotalItemCount => Items.Sum(i => i.Quantity);
    public string MsiLabel
    {
        get
        {
            var meses = SelectedPayment switch { "3 MSI" => 3, "6 MSI" => 6, "9 MSI" => 9, _ => 0 };
            return (meses == 0 || Total <= 0) ? string.Empty : $"{meses} pagos de ${Total / meses:F2}/mes";
        }
    }
    public bool EsPagoCredito => SelectedPayment == "Crédito";

    // Opciones MSI disponibles según el total:
    //   < $50   → solo Contado + Crédito
    //   $50–149 → + 3 MSI
    //   $150–299→ + 6 MSI
    //   ≥ $300  → + 9 MSI
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

    public CartViewModel(
        ICartService      cartService,
        IAuthService      authService,
        IPromocionService promoService,
        IEnvioService     envioService,
        IClienteService   clienteService)
    {
        _cartService    = cartService;
        _authService    = authService;
        _promoService   = promoService;
        _envioService   = envioService;
        _clienteService = clienteService;
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
        _ = LoadAddressesAsync();
    }

    private async Task LoadAddressesAsync()
    {
        if (!_authService.IsLoggedIn) return;
        var dirs = await _clienteService.GetDireccionesAsync(_authService.GetUserId());
        UserAddresses = new ObservableCollection<Address>(dirs);
        SelectedAddress = dirs.FirstOrDefault(d => d.IsDefault) ?? dirs.FirstOrDefault();
        OnPropertyChanged(nameof(TieneAddresses));
        OnPropertyChanged(nameof(SelectedAddressLabel));
    }

    [RelayCommand]
    private void ToggleAddressPicker() => ShowAddressPicker = !ShowAddressPicker;

    [RelayCommand]
    private void SelectAddress(Address address) => SelectedAddress = address;

    [RelayCommand]
    private async Task GoToAddressesAsync() =>
        await Shell.Current.GoToAsync("AddressesPage");

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
        OnPropertyChanged(nameof(PaymentOptions));

        // Resetear a Contado si la opción ya no aplica con el nuevo total
        if (SelectedPayment != "Contado" && SelectedPayment != "Crédito" &&
            !PaymentOptions.Contains(SelectedPayment))
            SelectedPayment = "Contado";
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

        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var dirStr = SelectedAddress is null ? string.Empty
                : $"{SelectedAddress.Street}, {SelectedAddress.City}, {SelectedAddress.State} {SelectedAddress.ZipCode}".Trim();

            // La modalidad de pago (Contado / MSI / Crédito) se elige en PaymentPage,
            // que es la pantalla de pago. El carrito solo entrega los importes.
            await Shell.Current.GoToAsync("PaymentPage",
                new Dictionary<string, object>
                {
                    ["ModalidadPago"]    = SelectedPayment,
                    ["Total"]            = Total,
                    ["Subtotal"]         = Subtotal,
                    ["Descuento"]        = Discount,
                    ["CostoEnvio"]       = CostoEnvio,
                    ["DireccionEntrega"] = dirStr
                });
        }
        finally { IsBusy = false; }
    }
}
