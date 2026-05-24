using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly IAuthService     _authService;
    private readonly ICartService     _cartService;

    private List<Product> _allProducts      = new();
    private List<Product> _filteredProducts = new();
    private const int PageSize = 20;

    [ObservableProperty] private ObservableCollection<Product> products = new();
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string selectedFilter = "TODO";
    [ObservableProperty] private string searchQuery = string.Empty;
    [ObservableProperty] private int cartItemCount;
    [ObservableProperty] private ObservableCollection<string> filters = new() { "TODO" };
    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int totalPages  = 1;

    public bool   HasNextPage => CurrentPage < TotalPages;
    public bool   HasPrevPage => CurrentPage > 1;
    public string PageLabel   => $"{CurrentPage} / {TotalPages}";

    public ProductsViewModel(
        IProductoService productoService,
        IAuthService authService,
        ICartService cartService)
    {
        _productoService = productoService;
        _authService     = authService;
        _cartService     = cartService;
    }

    public void RefreshCartCount() => CartItemCount = _cartService.GetItemCount();

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (IsRefreshing) return;
        IsRefreshing = true;
        try
        {
            var cats = await _productoService.GetCategoriasAsync();
            Filters = new ObservableCollection<string>(
                new[] { "TODO" }.Concat(cats.Select(c => c.ToUpper())));
            var items = await _productoService.GetProductosAsync();
            _allProducts = items;
            ApplyFilter();
        }
        finally { IsRefreshing = false; }
    }

    partial void OnSelectedFilterChanged(string value) => ApplyFilter();
    partial void OnSearchQueryChanged(string value)    => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = _allProducts.AsEnumerable();
        if (SelectedFilter != "TODO")
            filtered = filtered.Where(p =>
                p.Category.Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(SearchQuery))
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchQuery,  StringComparison.OrdinalIgnoreCase) ||
                p.Brand.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        _filteredProducts = filtered.ToList();
        CurrentPage = 1;
        RefreshPagedProducts();
    }

    private void RefreshPagedProducts()
    {
        TotalPages = Math.Max(1, (int)Math.Ceiling(_filteredProducts.Count / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        Products = new ObservableCollection<Product>(
            _filteredProducts.Skip((CurrentPage - 1) * PageSize).Take(PageSize));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(HasPrevPage));
        OnPropertyChanged(nameof(PageLabel));
    }

    [RelayCommand]
    private void NextPage()
    {
        if (!HasNextPage) return;
        CurrentPage++;
        RefreshPagedProducts();
    }

    [RelayCommand]
    private void PrevPage()
    {
        if (!HasPrevPage) return;
        CurrentPage--;
        RefreshPagedProducts();
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
                await Shell.Current.GoToAsync("AccountPage");
            else
                await Shell.Current.GoToAsync("LoginPage");
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
