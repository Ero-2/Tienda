using Microsoft.EntityFrameworkCore;
using MsPagos.Data;
using MsPagos.Events;
using MsPagos.Models;

namespace MsPagos.Services;

public class PagoService
{
    private readonly PagosDbContext _db;
    private readonly OpenPayService _openPay;
    private readonly RabbitMQPublisher _publisher;
    private readonly ILogger<PagoService> _logger;

    public PagoService(PagosDbContext db, OpenPayService openPay, RabbitMQPublisher publisher, ILogger<PagoService> logger)
    {
        _db = db;
        _openPay = openPay;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task ProcesarOrdenCreadaAsync(OrdenCreadaEvent evento)
    {
        _logger.LogInformation("Creando checkout para orden {OrdenId}", evento.OrdenId);

        var existente = await _db.Pagos.FirstOrDefaultAsync(p => p.OrdenId == evento.OrdenId);
        if (existente is not null)
        {
            _logger.LogInformation("Ya existe checkout para orden {OrdenId} (token {Token}). Se omite.",
                evento.OrdenId, existente.TokenCheckout);
            return;
        }

        var pago = new Pago
        {
            OrdenId       = evento.OrdenId,
            UsuarioId     = evento.ClienteId,
            Monto         = evento.Total,
            MesesMsi      = evento.MesesMsi,
            Estado        = "pendiente",
            TokenCheckout = Guid.NewGuid().ToString("N"),
            CreadoEn      = DateTime.UtcNow
        };

        _db.Pagos.Add(pago);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Checkout creado para orden {OrdenId}. Token: {Token}",
            evento.OrdenId, pago.TokenCheckout);
    }

    public async Task<CheckoutResponse?> ObtenerCheckoutPorTokenAsync(string token, string baseUrl)
    {
        var pago = await _db.Pagos.FirstOrDefaultAsync(p => p.TokenCheckout == token);
        if (pago is null) return null;

        return new CheckoutResponse
        {
            Token   = pago.TokenCheckout,
            OrdenId = pago.OrdenId,
            UsuarioId = pago.UsuarioId,
            Monto   = pago.Monto,
            MesesMsi = pago.MesesMsi,
            Estado  = pago.Estado,
            UrlPago = $"{baseUrl}/pagos/checkout/{pago.TokenCheckout}/confirmar"
        };
    }

    public async Task<(ConfirmacionPagoResponse? resultado, string? error)> ConfirmarPagoAsync(string token, TarjetaRequest tarjeta)
    {
        var pago = await _db.Pagos.FirstOrDefaultAsync(p => p.TokenCheckout == token);
        if (pago is null)
            return (null, "NO_ENCONTRADO");

        if (pago.Estado != "pendiente")
        {
            _logger.LogInformation("Pago de orden {OrdenId} ya procesado ({Estado}). Respuesta idempotente.",
                pago.OrdenId, pago.Estado);
            return (MapearConfirmacion(pago, yaProcesado: true), null);
        }

        // MSI: cobrar solo la primera cuota
        decimal montoCobrar = pago.MesesMsi > 0
            ? Math.Round(pago.Monto / pago.MesesMsi, 2)
            : pago.Monto;

        var resultado = await _openPay.ProcesarPagoConTarjetaAsync(montoCobrar, pago.MesesMsi, tarjeta);

        pago.Estado             = resultado.Estado;
        pago.ProcesadoEn        = DateTime.UtcNow;
        pago.RespuestaOpenpay   = resultado.Mensaje;
        pago.TarjetaEnmascarada = TarjetaValidator.Enmascarar(tarjeta.NumeroTarjeta);
        pago.MarcaTarjeta       = TarjetaValidator.DetectarMarca(tarjeta.NumeroTarjeta);

        if (resultado.Estado == "aprobado")
        {
            pago.IdTransaccionOpenpay = resultado.TransaccionId;
            pago.ReferenciaOpenpay    = resultado.Referencia;

            // Crear plan de pagos si es MSI
            if (pago.MesesMsi > 0)
            {
                var plan = new PlanPago
                {
                    OrdenId         = pago.OrdenId,
                    UsuarioId       = pago.UsuarioId,
                    TotalFinanciado = pago.Monto,
                    MesesMsi        = pago.MesesMsi,
                    CuotaMensual    = montoCobrar,
                    CuotasPagadas   = 1,
                    Estado          = pago.MesesMsi == 1 ? "completado" : "activo",
                    CreadoEn        = DateTime.UtcNow
                };
                _db.PlanPagos.Add(plan);
            }
        }

        await _db.SaveChangesAsync();
        await PublicarEventoAsync(pago, resultado);

        return (MapearConfirmacion(pago, yaProcesado: false), null);
    }

    private async Task PublicarEventoAsync(Pago pago, OpenPayResult resultado)
    {
        switch (resultado.Estado)
        {
            case "aprobado":
                await _publisher.PublicarAsync("tienda.pagos", "pago.aprobado", new PagoAprobadoEvent
                {
                    OrdenId       = pago.OrdenId,
                    TransaccionId = resultado.TransaccionId!,
                    Monto         = pago.Monto,
                    MesesMsi      = pago.MesesMsi,
                    ProcesadoEn   = DateTime.UtcNow
                });
                _logger.LogInformation("Pago APROBADO para orden {OrdenId}", pago.OrdenId);
                break;

            case "cancelado":
                await _publisher.PublicarAsync("tienda.pagos", "pago.cancelado", new PagoCanceladoEvent
                {
                    OrdenId     = pago.OrdenId,
                    Motivo      = resultado.Mensaje,
                    ProcesadoEn = DateTime.UtcNow
                });
                _logger.LogWarning("Pago CANCELADO para orden {OrdenId}", pago.OrdenId);
                break;

            default:
                await _publisher.PublicarAsync("tienda.pagos", "pago.rechazado", new PagoRechazadoEvent
                {
                    OrdenId      = pago.OrdenId,
                    ErrorCodigo  = resultado.CodigoError ?? "unknown",
                    ErrorMensaje = resultado.Mensaje,
                    ProcesadoEn  = DateTime.UtcNow
                });
                _logger.LogWarning("Pago RECHAZADO para orden {OrdenId}", pago.OrdenId);
                break;
        }
    }

    public async Task<List<PagoResponse>> ObtenerPagosPorOrdenAsync(int ordenId)
    {
        var pagos = await _db.Pagos.Where(p => p.OrdenId == ordenId).ToListAsync();
        return pagos.Select(MapearPagoResponse).ToList();
    }

    private static ConfirmacionPagoResponse MapearConfirmacion(Pago pago, bool yaProcesado) => new()
    {
        Token               = pago.TokenCheckout,
        OrdenId             = pago.OrdenId,
        Estado              = pago.Estado,
        Mensaje             = pago.RespuestaOpenpay ?? string.Empty,
        TransaccionId       = pago.IdTransaccionOpenpay,
        Referencia          = pago.ReferenciaOpenpay,
        TarjetaEnmascarada  = pago.TarjetaEnmascarada,
        MarcaTarjeta        = pago.MarcaTarjeta,
        YaProcesado         = yaProcesado,
        ProcesadoEn         = pago.ProcesadoEn
    };

    private static PagoResponse MapearPagoResponse(Pago p) => new()
    {
        Id                   = p.Id,
        OrdenId              = p.OrdenId,
        UsuarioId            = p.UsuarioId,
        Monto                = p.Monto,
        MesesMsi             = p.MesesMsi,
        Estado               = p.Estado,
        TokenCheckout        = p.TokenCheckout,
        IdTransaccionOpenpay = p.IdTransaccionOpenpay,
        ReferenciaOpenpay    = p.ReferenciaOpenpay,
        TarjetaEnmascarada   = p.TarjetaEnmascarada,
        MarcaTarjeta         = p.MarcaTarjeta,
        CreadoEn             = p.CreadoEn,
        ProcesadoEn          = p.ProcesadoEn
    };

    // ── Planes MSI ───────────────────────────────────────────────

    public async Task<List<PlanPagoResponse>> ObtenerPlanesPorUsuarioAsync(int usuarioId)
    {
        var planes = await _db.PlanPagos
            .Where(p => p.UsuarioId == usuarioId && p.Estado == "activo")
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        return planes.Select(p => new PlanPagoResponse
        {
            Id              = p.Id,
            OrdenId         = p.OrdenId,
            TotalFinanciado  = p.TotalFinanciado,
            MesesMsi        = p.MesesMsi,
            CuotaMensual    = p.CuotaMensual,
            CuotasPagadas   = p.CuotasPagadas,
            SaldoPendiente  = Math.Round(p.CuotaMensual * (p.MesesMsi - p.CuotasPagadas), 2),
            Estado          = p.Estado,
            CreadoEn        = p.CreadoEn
        }).ToList();
    }

    public async Task<(PlanPagoResponse? resultado, string? error)> PagarCuotaAsync(int planId, int usuarioId, TarjetaRequest tarjeta)
    {
        var plan = await _db.PlanPagos.FindAsync(planId);
        if (plan is null)                     return (null, "Plan no encontrado");
        if (plan.UsuarioId != usuarioId)      return (null, "No autorizado");
        if (plan.Estado != "activo")          return (null, "Plan ya completado");
        if (plan.CuotasPagadas >= plan.MesesMsi) return (null, "Todas las cuotas ya fueron pagadas");

        var resultado = await _openPay.ProcesarPagoConTarjetaAsync(plan.CuotaMensual, 0, tarjeta);
        if (resultado.Estado != "aprobado")
            return (null, $"Pago rechazado: {resultado.Mensaje}");

        plan.CuotasPagadas++;
        if (plan.CuotasPagadas >= plan.MesesMsi)
            plan.Estado = "completado";

        await _db.SaveChangesAsync();

        return (new PlanPagoResponse
        {
            Id              = plan.Id,
            OrdenId         = plan.OrdenId,
            TotalFinanciado  = plan.TotalFinanciado,
            MesesMsi        = plan.MesesMsi,
            CuotaMensual    = plan.CuotaMensual,
            CuotasPagadas   = plan.CuotasPagadas,
            SaldoPendiente  = Math.Round(plan.CuotaMensual * (plan.MesesMsi - plan.CuotasPagadas), 2),
            Estado          = plan.Estado,
            CreadoEn        = plan.CreadoEn
        }, null);
    }
}
