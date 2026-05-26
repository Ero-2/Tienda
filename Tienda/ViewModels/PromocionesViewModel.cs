using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class PromocionesViewModel : ObservableObject
{
    private readonly IPromocionService _promoService;
    private readonly ICartService _cartService;

    [ObservableProperty]
    private ObservableCollection<Promocion> promociones = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private decimal totalCarrito;

    [ObservableProperty]
    private decimal descuentoAplicado;

    [ObservableProperty]
    private decimal totalAPagar;

    [ObservableProperty]
    private string motivoDescuento = "Sin descuento aplicable";

    [ObservableProperty]
    private bool tieneDescuento;

    [ObservableProperty]
    private bool hayItemsEnCarrito;

    public PromocionesViewModel(IPromocionService promoService, ICartService cartService)
    {
        _promoService = promoService;
        _cartService = cartService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;

        var promos = await _promoService.GetPromocionesActivasAsync();
        Promociones = new ObservableCollection<Promocion>(promos);

        var items = _cartService.GetItems().ToList();
        HayItemsEnCarrito = items.Count > 0;
        TotalCarrito = items.Sum(i => i.Product.Price * i.Quantity);

        if (TotalCarrito > 0)
        {
            decimal subtotalElectronicos = items
                .Where(i => i.Product.Category?.ToLower().Contains("electr") == true)
                .Sum(i => i.Product.Price * i.Quantity);
            decimal subtotalOtros = TotalCarrito - subtotalElectronicos;

            var resultado = await _promoService.CalcularDescuentoAsync(subtotalElectronicos, subtotalOtros);
            if (resultado is not null)
            {
                DescuentoAplicado = resultado.DescuentoAplicado;
                TotalAPagar = resultado.TotalAPagar;
                MotivoDescuento = resultado.Motivo;
                TieneDescuento = resultado.DescuentoAplicado > 0;
            }
        }
        else
        {
            DescuentoAplicado = 0;
            TotalAPagar = 0;
            MotivoDescuento = "Agrega productos al carrito";
            TieneDescuento = false;
        }

        IsLoading = false;
    }
}
