// Tienda/ViewModels/ProductDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

[QueryProperty(nameof(Product), "Product")]
public partial class ProductDetailViewModel : ObservableObject
{
    private readonly ICartService _cartService;

    [ObservableProperty]
    private Product? product;

    [ObservableProperty]
    private string selectedSize = string.Empty;

    [ObservableProperty]
    private bool isFavorite;

    public List<string> AvailableSizes { get; } =
        new() { "XS", "S", "M", "L", "XL", "XXL" };

    public ProductDetailViewModel(ICartService cartService)
    {
        _cartService = cartService;
    }

    [RelayCommand]
    private async Task AddToCartAsync()
    {
        try
        {
            if (product is null)
            {
                await Shell.Current.DisplayAlert("Error", "Producto no cargado. Inténtalo de nuevo.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedSize))
            {
                await Shell.Current.DisplayAlert("THE DROP", "Selecciona una talla.", "OK");
                return;
            }

            _cartService.AddItem(product, SelectedSize, 1);
            await Shell.Current.DisplayAlert("✓ AGREGADO",
                $"{product.Name} · Talla {SelectedSize}", "Ver carrito");
            await Shell.Current.GoToAsync("CartPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo agregar al carrito:\n{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void ToggleFavorite() => IsFavorite = !IsFavorite;

    [RelayCommand]
    private async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}