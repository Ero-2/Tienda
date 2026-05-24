using Tienda.Models;

namespace Tienda.Services;

public interface IPromocionService
{
    Task<List<Promocion>> GetPromocionesActivasAsync();
    Task<DescuentoResponse?> CalcularDescuentoAsync(decimal total, bool esSoloElectronica);
}
