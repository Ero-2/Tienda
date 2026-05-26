using Tienda.Models;

namespace Tienda.Services;

public interface IEnvioService
{
    Task<EnvioRastreoResponse?> GetRastreoAsync(int orderId);
    Task<CostoEnvioDto>         CalcularCostoAsync(decimal subtotal);
}

public class CostoEnvioDto
{
    public decimal CostoEnvio       { get; set; }
    public bool    EsGratis         { get; set; }
    public string  MensajePromocion { get; set; } = string.Empty;
}
