// Models/Order.cs
namespace Tienda.Models;

public class Order
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
}