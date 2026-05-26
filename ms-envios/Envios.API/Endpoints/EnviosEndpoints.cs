using Envios.API.Data;
using Envios.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Envios.API.Endpoints
{
    public static class EnviosEndpoints
    {
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/envios");

            // GET: Rastrear pedido por OrderId (Rastreo en todo momento)
            group.MapGet("/rastreo/{orderId}", async (int orderId, AppDbContext db) =>
            {
                var envio = await db.Envios.FirstOrDefaultAsync(e => e.OrderId == orderId);

                // Manejo de responses: 404 Si no existe
                if (envio == null) return Results.NotFound(new { mensaje = "Envío no encontrado" });

                var historial = await db.HistorialEstados
                    .Where(h => h.EnvioId == envio.Id)
                    .OrderByDescending(h => h.FechaRegistrado)
                    .ToListAsync();

                // Manejo de responses: 200 OK
                return Results.Ok(new { Envio = envio, Historial = historial });
            });

            // POST: Crear un nuevo envío
            group.MapPost("/", async (Envio nuevoEnvio, AppDbContext db) =>
            {
                // IDEMPOTENCIA: Si RabbitMQ manda el evento 2 veces, evitamos duplicados
                var existe = await db.Envios.AnyAsync(e => e.OrderId == nuevoEnvio.OrderId);
                if (existe) return Results.BadRequest(new { mensaje = "El envío para esta orden ya existe." }); // 400 Bad Request

                db.Envios.Add(nuevoEnvio);
                await db.SaveChangesAsync();

                // Guardar el primer paso en el historial
                db.HistorialEstados.Add(new HistorialEstado { EnvioId = nuevoEnvio.Id, Estado = nuevoEnvio.Estado });
                await db.SaveChangesAsync();

                // Manejo de responses: 201 Created
                return Results.Created($"/api/envios/rastreo/{nuevoEnvio.OrderId}", nuevoEnvio);
            });

            // PUT: Actualizar estado (Ej: En camino, Entregado, Reprogramado)
            group.MapPut("/{id}/estado", async (int id, string nuevoEstado, AppDbContext db) =>
            {
                var envio = await db.Envios.FindAsync(id);
                if (envio == null) return Results.NotFound(new { mensaje = "Envío no encontrado" });

                envio.Estado = nuevoEstado;
                db.HistorialEstados.Add(new HistorialEstado { EnvioId = envio.Id, Estado = nuevoEstado });

                await db.SaveChangesAsync();
                return Results.Ok(envio);
            });

            // GET: Calcular costo de envío (envío gratis >= $50)
            group.MapGet("/calcular-costo", (decimal subtotal) =>
            {
                const decimal umbralGratis = 50m;
                const decimal costoBase    = 9.99m;

                bool esGratis = subtotal >= umbralGratis;
                return Results.Ok(new
                {
                    CostoEnvio          = esGratis ? 0m : costoBase,
                    EsGratis            = esGratis,
                    MensajePromocion    = esGratis
                        ? "Envío gratis por compra mayor a $50"
                        : $"Envío gratis en compras mayores a $50 (faltan ${umbralGratis - subtotal:F2})"
                });
            });
        }
    }
}