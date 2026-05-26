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

    // Categorías que requieren talla de ropa
    private static readonly HashSet<string> _ropaKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "hoodie", "hoodies", "pants", "shirt", "shirts", "top", "tops",
        "jacket", "jackets", "coat", "jeans", "shorts", "sweater", "sweaters",
        "dress", "dresses", "skirt", "blouse", "tee", "polo", "ropa", "playera",
        "sudadera", "pantalon", "chamarra", "clothing", "apparel"
    };

    // Categorías que requieren talla de calzado
    private static readonly HashSet<string> _calzadoKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "sneakers", "sneaker", "shoes", "shoe", "boots", "boot",
        "sandals", "sandal", "tenis", "zapatillas", "calzado", "footwear"
    };

    public bool EsCalzado    => ContainsAny(Product?.Category, _calzadoKeywords);
    public bool EsRopa       => !EsCalzado && ContainsAny(Product?.Category, _ropaKeywords);
    public bool RequieresTalla => EsCalzado || EsRopa;
    public string TallaLabel => EsCalzado ? "NÚMERO" : "TALLA";

    public List<string> AvailableSizes => EsCalzado
        ? new() { "24", "25", "26", "27", "28", "29", "30" }
        : new() { "XS", "S", "M", "L", "XL", "XXL" };

    public ProductDetailViewModel(ICartService cartService)
    {
        _cartService = cartService;
    }

    partial void OnProductChanged(Product? value)
    {
        OnPropertyChanged(nameof(EsCalzado));
        OnPropertyChanged(nameof(EsRopa));
        OnPropertyChanged(nameof(RequieresTalla));
        OnPropertyChanged(nameof(TallaLabel));
        OnPropertyChanged(nameof(AvailableSizes));
        SelectedSize = string.Empty;
    }

    [RelayCommand]
    private async Task AddToCartAsync()
    {
        if (product is null)
        {
            await Shell.Current.DisplayAlert("Error", "Producto no cargado. Inténtalo de nuevo.", "OK");
            return;
        }

        if (RequieresTalla && string.IsNullOrWhiteSpace(SelectedSize))
        {
            var tipo = EsCalzado ? "número" : "talla";
            await Shell.Current.DisplayAlert("THE DROP", $"Selecciona un {tipo}.", "OK");
            return;
        }

        bool added = _cartService.AddItem(product, SelectedSize, 1);
        if (!added)
        {
            await Shell.Current.DisplayAlert(
                "Sin stock",
                $"Solo hay {product.Stock} unidades disponibles de este producto.",
                "OK");
            return;
        }

        var detalle = RequieresTalla
            ? $"{product.Name} · {(EsCalzado ? "Núm." : "Talla")} {SelectedSize}"
            : product.Name;

        await Shell.Current.DisplayAlert("✓ AGREGADO", detalle, "Ver carrito");
        await Shell.Current.GoToAsync("CartPage");
    }

    [RelayCommand]
    private void ToggleFavorite() => IsFavorite = !IsFavorite;

    [RelayCommand]
    private async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");

    private static bool ContainsAny(string? value, HashSet<string> keywords)
    {
        if (string.IsNullOrEmpty(value)) return false;
        // Split en palabras para evitar falsos positivos (ej: "laptops" contiene "top")
        var words = value.Split(new[] { '-', ' ', '_', '/' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Any(w => keywords.Contains(w));
    }
}
