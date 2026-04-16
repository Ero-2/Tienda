using System;
using System.Collections.Generic;
using System.Text;

namespace Tienda.Models
{
    public class CartItem
    {
        public Product Product { get; set; } = new();
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
    }
}
