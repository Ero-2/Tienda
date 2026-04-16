// Tienda/ViewModels/ProductsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Tienda.Data;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private List<Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string selectedFilter = "TODO";

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private int cartItemCount;

    public List<string> Filters { get; } =
        new() { "TODO", "HOODIES", "PANTS", "SNEAKERS", "CAPS" };

    public ProductsViewModel(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (IsRefreshing) return;
        IsRefreshing = true;

        try
        {
            await Task.Run(() => _context.Database.EnsureCreated());
            var items = await _context.Products.ToListAsync();

            if (!items.Any())
            {
                var seed = new List<Product>
                {
                    new() { Name = "OVERSIZED TECH HOODIE V2", Brand = "Aethel", Category = "HOODIES", Price = 115.00m, ImageUrl = "hoodie.png", IsNew = true },
                    new() { Name = "UTILITY CARGO PANTS", Brand = "Techwear Labs", Category = "PANTS", Price = 130.00m, ImageUrl = "cargo.png", IsHot = true },
                    new() { Name = "NEON BOLT SNEAKERS", Brand = "Runic Customs", Category = "SNEAKERS", Price = 199.99m, ImageUrl = "sneakers.png", IsHot = true },
                    new() { Name = "CYBERFUTURIST CAP", Brand = "Voidwear", Category = "CAPS", Price = 45.50m, ImageUrl = "cap.png", IsNew = true },
                };
                _context.Products.AddRange(seed);
                await _context.SaveChangesAsync();
                items = await _context.Products.ToListAsync();
            }

            _allProducts = items;
            ApplyFilter();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnSelectedFilterChanged(string value) => ApplyFilter();
    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = _allProducts.AsEnumerable();

        if (SelectedFilter != "TODO")
            filtered = filtered.Where(p => p.Category == SelectedFilter);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Brand.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        Products = new ObservableCollection<Product>(filtered);
    }

    [RelayCommand]
    private async Task ProductTappedAsync(Product product)
    {
        try
        {
            await Shell.Current.GoToAsync("ProductDetailPage",
                new Dictionary<string, object> { ["Product"] = product });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GoToAccountAsync()
    {
        try
        {
            if (_authService.IsLoggedIn)
                await Shell.Current.GoToAsync("AccountPage"); // ✅ registrada en AppShell
            else
                await Shell.Current.GoToAsync("LoginPage");   // ✅ sin /// y con L mayúscula
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GoToCartAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("CartPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}");
        }
    }
}