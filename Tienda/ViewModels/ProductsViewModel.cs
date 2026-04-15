using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Tienda.Data;
using Tienda.Models;
using System.Collections.ObjectModel;

namespace Tienda.ViewModels;

// AGREGAMOS 'partial' AQUÍ
public partial class ProductsViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    // Los campos privados para ObservableProperty deben ser camelCase (products) 
    // o llevar guion bajo (_products). El generador creará la propiedad "Products".
    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private bool isRefreshing;

    public ProductsViewModel(AppDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
        _ = LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsRefreshing = true; // El generador crea la propiedad con Mayúscula

        var items = await _context.Products.ToListAsync();

        if (!items.Any())
        {
            _context.Products.Add(new Product { Name = "OVERSIZED HOODIE", Brand = "YNW", Price = 89.99m, ImageUrl = "hoodie.jpg" });
            _context.Products.Add(new Product { Name = "CARGO PANTS V2", Brand = "TECH", Price = 120.00m, ImageUrl = "cargo.jpg" });
            await _context.SaveChangesAsync();
            items = await _context.Products.ToListAsync();
        }

        Products = new ObservableCollection<Product>(items); // Usamos la propiedad generada
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task GoToAccount()
    {
        // El nombre de la ruta debe coincidir con el registrado en AppShell
        await Shell.Current.GoToAsync("AccountPage");
    }
}