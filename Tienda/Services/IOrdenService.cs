// Tienda/Services/IOrdenService.cs
using Tienda.Models;

namespace Tienda.Services;

public interface IOrdenService
{
    Task<OrdenResponse?> CrearOrdenAsync(List<CartItem> items, string modalidadPago, string cardToken);
    Task<List<OrdenResponse>> ObtenerMisOrdenesAsync();
}

public class OrdenResponse
{
    public int Id { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DescuentoPct { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal Total { get; set; }
    public string ModalidadPago { get; set; } = string.Empty;
    public int MesesMsi { get; set; }
    public DateTime CreadoEn { get; set; }
}
