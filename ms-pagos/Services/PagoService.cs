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
        _logger.LogInformation("Procesando pago para orden {OrdenId}", evento.OrdenId);

        // 1. Crear registro de pago pendiente
        var pago = new Pago
        {
            OrdenId = evento.OrdenId,
            UsuarioId = evento.ClienteId,
            Monto = evento.Total,
            MesesMsi = evento.MesesMsi,
            Estado = "pendiente",
            CreadoEn = DateTime.UtcNow
        };

        _db.Pagos.Add(pago);
        await _db.SaveChangesAsync();

        // 2. Procesar pago en OpenPay
        var resultado = await _openPay.ProcesarPagoAsync(
            pago.Monto, pago.MesesMsi, evento.CardToken, evento.OrdenId);

        // 3. Actualizar estado del pago
        pago.ProcesadoEn = DateTime.UtcNow;
        pago.RespuestaOpenpay = resultado.Mensaje;

        if (resultado.Exitoso)
        {
            pago.Estado = "aprobado";
            pago.IdTransaccionOpenpay = resultado.TransaccionId;
            pago.ReferenciaOpenpay = resultado.Referencia;

            await _db.SaveChangesAsync();

            // 4a. Publicar PagoAprobado
            await _publisher.PublicarAsync("tienda.pagos", "pago.aprobado", new PagoAprobadoEvent
            {
                OrdenId = evento.OrdenId,
                TransaccionId = resultado.TransaccionId!,
                Monto = pago.Monto,
                MesiMsi = pago.MesesMsi,
                ProcesadoEn = DateTime.UtcNow
            });

            _logger.LogInformation("Pago aprobado para orden {OrdenId}", evento.OrdenId);
        }
        else
        {
            pago.Estado = "rechazado";
            await _db.SaveChangesAsync();

            // 4b. Publicar PagoRechazado
            await _publisher.PublicarAsync("tienda.pagos", "pago.rechazado", new PagoRechazadoEvent
            {
                OrdenId = evento.OrdenId,
                ErrorCodigo = resultado.ErrorCodigo ?? "9999",
                ErrorMensaje = resultado.Mensaje,
                ProcesadoEn = DateTime.UtcNow
            });

            _logger.LogWarning("Pago rechazado para orden {OrdenId}", evento.OrdenId);
        }
    }

    public async Task<List<PagoResponse>> ObtenerPagosPorOrdenAsync(int ordenId)
    {
        var pagos = await _db.Pagos
            .Where(p => p.OrdenId == ordenId)
            .ToListAsync();

        return pagos.Select(p => new PagoResponse
        {
            Id = p.Id,
            OrdenId = p.OrdenId,
            UsuarioId = p.UsuarioId,
            Monto = p.Monto,
            MesesMsi = p.MesesMsi,
            Estado = p.Estado,
            IdTransaccionOpenpay = p.IdTransaccionOpenpay,
            ReferenciaOpenpay = p.ReferenciaOpenpay,
            CreadoEn = p.CreadoEn,
            ProcesadoEn = p.ProcesadoEn
        }).ToList();
    }
}