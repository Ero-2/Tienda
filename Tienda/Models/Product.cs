using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tienda.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;   // HOODIES, PANTS, SNEAKERS, CAPS
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public bool IsHot { get; set; }
        public int Stock { get; set; } = 10;
    }
}
