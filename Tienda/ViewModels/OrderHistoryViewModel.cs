using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Tienda.Models;

namespace Tienda.ViewModels;

public partial class OrderHistoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Order> orders = new();

    [ObservableProperty]
    private bool isEmpty = true;

    public OrderHistoryViewModel()
    {
        LoadOrders();
    }

    // ❌ ELIMINADO: Estas líneas causaban el error porque [ObservableProperty] ya las genera.
    // public ObservableCollection<Order> Orders { get; private set; }
    // public bool IsEmpty { get; private set; }

    private void LoadOrders()
    {
        // ✅ Al usar 'Orders', estás usando la propiedad generada automáticamente
        Orders = new ObservableCollection<Order>
        {
            new Order
            {
                Id = "#ORD-001",
                Date = DateTime.Now.AddDays(-3),
                Total = 299.99m,
                Status = "ENTREGADO",
                Items = new List<CartItem>()
            },
            new Order
            {
                Id = "#ORD-002",
                Date = DateTime.Now.AddDays(-10),
                Total = 549.00m,
                Status = "EN CAMINO",
                Items = new List<CartItem>()
            },
            new Order
            {
                Id = "#ORD-003",
                Date = DateTime.Now.AddDays(-21),
                Total = 189.50m,
                Status = "ENTREGADO",
                Items = new List<CartItem>()
            }
        };

        // ✅ Al usar 'IsEmpty', estás usando la propiedad generada automáticamente
        IsEmpty = Orders.Count == 0;
    }
}