// Tienda/Services/CartService.cs
using Tienda.Models;

namespace Tienda.Services;

public class CartService : ICartService
{
    private readonly List<CartItem> _items = new();

    public bool AddItem(Product product, string size, int qty)
    {
        var existing = _items.FirstOrDefault(i =>
            i.Product.Id == product.Id && i.Size == size);

        int currentQty = existing?.Quantity ?? 0;
        if (currentQty + qty > product.Stock) return false;

        if (existing is not null)
            existing.Quantity += qty;
        else
            _items.Add(new CartItem { Product = product, Size = size, Quantity = qty });

        return true;
    }

    public void RemoveItem(CartItem item) => _items.Remove(item);

    public List<CartItem> GetItems() => _items.ToList();

    public int GetItemCount() => _items.Sum(i => i.Quantity);

    public void Clear() => _items.Clear();
}