using System;
using System.Collections.Generic;
using System.Text;

namespace Tienda.Services
{
    public interface ICartService
    {
        void AddItem(Tienda.Models.Product product, string size, int qty);
        void RemoveItem(Tienda.Models.CartItem item);
        List<Tienda.Models.CartItem> GetItems();
        int GetItemCount();
        void Clear();
    }
}
