using Tienda.Models;

namespace Tienda.Services;

public interface IEnvioService
{
    Task<EnvioRastreoResponse?> GetRastreoAsync(int orderId);
}
