using MsPagos.Models;

namespace MsPagos.Services;

public class OpenPayService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OpenPayService> _logger;

    public OpenPayService(IConfiguration config, ILogger<OpenPayService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<OpenPayResult> ProcesarPagoConTarjetaAsync(decimal monto, int mesesMsi, TarjetaRequest tarjeta)
    {
        await Task.Delay(300);

        var probAprobado         = _config.GetValue<int>("OpenPay:ProbabilidadAprobado", 80);
        var probRechazado        = _config.GetValue<int>("OpenPay:ProbabilidadRechazado", 15);
        var probFallaTransitoria = _config.GetValue<int>("OpenPay:ProbabilidadFallaTransitoria", 0);

        for (var intento = 1; intento <= 3; intento++)
        {
            try
            {
                return Simular(probAprobado, probRechazado, probFallaTransitoria);
            }
            catch (OpenPayTransitorioException ex)
            {
                _logger.LogWarning("Falla transitoria intento {Intento}: {Msg}", intento, ex.Message);
                if (intento == 3) throw;
                await Task.Delay(200 * intento);
            }
        }

        throw new InvalidOperationException("No debería llegar aquí");
    }

    private static OpenPayResult Simular(int probAprobado, int probRechazado, int probFalla)
    {
        var rand = Random.Shared.Next(1, 101);

        if (rand <= probFalla)
            throw new OpenPayTransitorioException("Error de red simulado");

        if (rand <= probFalla + probAprobado)
            return new OpenPayResult
            {
                Estado        = "aprobado",
                TransaccionId = $"sim_txn_{Guid.NewGuid():N}",
                Referencia    = $"sim_ref_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Mensaje       = "Pago aprobado"
            };

        if (rand <= probFalla + probAprobado + probRechazado)
            return new OpenPayResult
            {
                Estado      = "rechazado",
                CodigoError = "3001",
                Mensaje     = "Tarjeta declinada"
            };

        return new OpenPayResult
        {
            Estado      = "cancelado",
            CodigoError = "2004",
            Mensaje     = "Pago cancelado por el emisor"
        };
    }
}

public class OpenPayResult
{
    public string  Estado        { get; set; } = string.Empty;
    public string? TransaccionId { get; set; }
    public string? Referencia    { get; set; }
    public string? CodigoError   { get; set; }
    public string  Mensaje       { get; set; } = string.Empty;
}

public class OpenPayTransitorioException : Exception
{
    public OpenPayTransitorioException(string message) : base(message) { }
}
