using CommunityToolkit.Mvvm.ComponentModel;

namespace Tienda.Models
{
    public partial class CartItem : ObservableObject
    {
        public Product Product { get; set; } = new();
        public string  Size    { get; set; } = string.Empty;

        [ObservableProperty]
        private int quantity = 1;

        // Se recalcula y notifica cuando Quantity cambia
        partial void OnQuantityChanged(int value) =>
            OnPropertyChanged(nameof(LineTotal));

        public decimal LineTotal => Product.Price * Quantity;
    }
}
