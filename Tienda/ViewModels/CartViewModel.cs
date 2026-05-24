// Tienda/ViewModels/CartViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class CartViewModel : ObservableObject
{
    private readonly ICartService _cartService;
    private readonly IAuthService _authService;
    private readonly IPromocionService _promoService;

    [ObservableProperty]
    private ObservableCollection<CartItem> items = new();

    [ObservableProperty]
    private decimal subtotal;

    [ObservableProperty]
    private decimal discount;

    [ObservableProperty]
    private decimal total;

    [ObservableProperty]
    private string selectedPayment = "Contado";

    [ObservableProperty]
    private string discountLabel = string.Empty;

    public List<string> PaymentOptions { get; } =
        new() { "Contado", "3 MSI", "6 MSI", "12 MSI" };

    public CartViewModel(ICartService cartService, IAuthService authService, IPromocionService promoService)
    {
        _cartService = cartService;
        _authService = authService;
        _promoService = promoService;
        LoadCart();
    }

    public void LoadCart()
    {
        Items = new ObservableCollection<CartItem>(_cartService.GetItems());
        RecalculateTotals();
        _ = ApplyPromoAsync();
    }

    private void RecalculateTotals()
    {
        Subtotal = Items.Sum(i => i.Product.Price * i.Quantity);
        Discount = 0;
        DiscountLabel = string.Empty;
        Total = Subtotal;
    }

    private async Task ApplyPromoAsync()
    {
        if (Subtotal <= 0) return;

        bool esSoloElectronica = Items.Count > 0 &&
            Items.All(i => i.Product.Category?.ToLower().Contains("electr") == true);

        var resultado = await _promoService.CalcularDescuentoAsync(Subtotal, esSoloElectronica);
        if (resultado is null) return;

        Discount = Math.Round(resultado.DescuentoAplicado, 2);
        DiscountLabel = resultado.Motivo;
        Total = resultado.TotalAPagar;
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
        OnPropertyChanged(nameof(Items));
        RecalculateTotals();
        _ = ApplyPromoAsync();
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
        }
        else
        {
            _cartService.RemoveItem(item);
            Items.Remove(item);
        }
        RecalculateTotals();
        _ = ApplyPromoAsync();
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
            if (goLogin)
                await Shell.Current.GoToAsync("LoginPage");
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
                ["Total"]         = Total
            });
    }
}
