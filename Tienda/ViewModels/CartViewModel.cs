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

    public CartViewModel(ICartService cartService)
    {
        _cartService = cartService;
        LoadCart();
    }

    public void LoadCart()
    {
        Items = new ObservableCollection<CartItem>(_cartService.GetItems());
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = Items.Sum(i => i.Product.Price * i.Quantity);

        if (Subtotal > 200)
        {
            Discount = Math.Round(Subtotal * 0.10m, 2);
            DiscountLabel = "Descuento automático 10%";
        }
        else
        {
            Discount = 0;
            DiscountLabel = string.Empty;
        }

        Total = Subtotal - Discount;
    }

    [RelayCommand]
    private void IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
        OnPropertyChanged(nameof(Items));
        RecalculateTotals();
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
    }

    [RelayCommand]
    private async Task ConfirmPurchaseAsync()
    {
        if (!Items.Any())
        {
            await Shell.Current.DisplayAlert("Carrito vacío", "Agrega productos primero.", "OK");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "CONFIRMAR ORDEN",
            $"Total: ${Total:F2} · Pago: {SelectedPayment}",
            "CONFIRMAR", "CANCELAR");

        if (!confirm) return;

        var orderId = $"DROP-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        _cartService.Clear();
        LoadCart(); // Recarga carrito vacío

        await Shell.Current.GoToAsync("OrderTrackingPage",
            new Dictionary<string, object> { ["OrderId"] = orderId });
    }
}