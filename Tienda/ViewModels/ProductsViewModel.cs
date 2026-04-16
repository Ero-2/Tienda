using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Tienda.Data;
using Tienda.Models;

namespace Tienda.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private bool isRefreshing;

    public ProductsViewModel(AppDbContext context)
    {
        _context = context;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (IsRefreshing) return; // Evita llamadas dobles

        IsRefreshing = true;

        // 1. Creamos la base de datos sin congelar la pantalla
        await Task.Run(() => _context.Database.EnsureCreated());

        // 2. Buscamos los productos
        var items = await _context.Products.ToListAsync();

        // 3. SEED: Si no hay nada, agregamos el stock inicial
        if (!items.Any())
        {
            var productList = new List<Product>
            {
                new Product { Name = "OVERSIZED TECH HOODIE V2", Brand = "Aethel", Price = 115.00m, ImageUrl = "dotnet_bot.png" },
                new Product { Name = "UTILITY CARGO PANTS", Brand = "Techwear Labs", Price = 130.00m, ImageUrl = "dotnet_bot.png" },
                new Product { Name = "NEON BOLT SNEAKERS", Brand = "Runic Customs", Price = 199.99m, ImageUrl = "dotnet_bot.png" },
                new Product { Name = "CYBERFUTURIST CAP", Brand = "Voidwear", Price = 45.50m, ImageUrl = "dotnet_bot.png" }
            };

            _context.Products.AddRange(productList);
            await _context.SaveChangesAsync();
            items = await _context.Products.ToListAsync();
        }

        // 4. Actualizamos la interfaz
        Products = new ObservableCollection<Product>(items);
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task GoToAccount()
    {
        await Shell.Current.GoToAsync("AccountPage");
    }
}