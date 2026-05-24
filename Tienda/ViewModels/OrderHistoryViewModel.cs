using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class OrderHistoryViewModel : ObservableObject
{
    private readonly IOrdenService _ordenService;

    [ObservableProperty]
    private ObservableCollection<Order> orders = new();

    [ObservableProperty]
    private bool isEmpty = true;

    [ObservableProperty]
    private bool isLoading = false;

    public OrderHistoryViewModel(IOrdenService ordenService)
    {
        _ordenService = ordenService;
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        IsLoading = true;
        try
        {
            var resultado = await _ordenService.ObtenerMisOrdenesAsync();
            Orders = new ObservableCollection<Order>(
                resultado.Select(o => new Order
                {
                    Id     = $"#{o.Id}",
                    Date   = o.CreadoEn,
                    Total  = o.Total,
                    Status = MapEstado(o.Estado),
                    Items  = new List<CartItem>()
                })
            );
            IsEmpty = Orders.Count == 0;
        }
        catch
        {
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string MapEstado(string estado) => estado switch
    {
        "confirmada" => "CONFIRMADA",
        "cancelada"  => "CANCELADA",
        "pendiente"  => "PROCESANDO",
        _            => estado.ToUpper()
    };
}