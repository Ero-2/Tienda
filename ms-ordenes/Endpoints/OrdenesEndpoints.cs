using Microsoft.EntityFrameworkCore;
using MsOrdenes.Data;
using MsOrdenes.Models;
using MsOrdenes.Services;
using System.Text.Json;

namespace MsOrdenes.Endpoints;

public static class OrdenesEndpoints
{
    public static void MapOrdenesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/ordenes")
            .WithTags("Ordenes");

        // POST /ordenes - Crear orden con idempotencia
        group.MapPost("/", async (
            HttpContext http,
            CrearOrdenRequest request,
            OrdenService service,
            OrdenesDbContext db) =>
        {
            // Validar request
            if (request.Items is null || request.Items.Count == 0)
                return Results.BadRequest(new { error = "La orden debe tener al menos un item" });

            if (request.UsuarioId <= 0)
                return Results.BadRequest(new { error = "El usuarioId no es válido" });

            if (!new[] { "contado", "3msi", "6msi", "12msi" }.Contains(request.ModalidadPago))
                return Results.BadRequest(new { error = "La modalidad de pago no es válida. Use: contado, 3msi, 6msi, 12msi" });

            // Verificar Idempotency-Key
            if (!http.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey)
                || string.IsNullOrEmpty(idempotencyKey))
                return Results.BadRequest(new { error = "Se requiere el header 'Idempotency-Key'" });

            // Verificar si ya existe esa clave
            var existente = await db.Idempotencias
                .FirstOrDefaultAsync(i => i.IdempotencyKey == idempotencyKey.ToString());

            if (existente is not null)
            {
                var ordenExistente = JsonSerializer.Deserialize<OrdenResponse>(existente.ResponseJson);
                return Results.Ok(ordenExistente);
            }

            try
            {
                var orden = await service.CrearOrdenAsync(request);

                db.Idempotencias.Add(new OutboxIdempotencia
                {
                    IdempotencyKey = idempotencyKey.ToString(),
                    Endpoint = "POST /ordenes",
                    ResponseJson = JsonSerializer.Serialize(orden),
                    CreadoEn = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                return Results.Created($"/ordenes/{orden.Id}", orden);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    title: "Error al crear la orden",
                    statusCode: 500
                );
            }
        })
        .WithName("CrearOrden")
        .WithSummary("Crea una nueva orden aplicando descuentos automáticos");

        // GET /ordenes/{id}
        group.MapGet("/{id:int}", async (int id, OrdenService service) =>
        {
            if (id <= 0)
                return Results.BadRequest(new { error = "El id debe ser mayor a 0" });

            try
            {
                var orden = await service.ObtenerOrdenPorIdAsync(id);
                return orden is null
                    ? Results.NotFound(new { error = $"No se encontró la orden con id {id}" })
                    : Results.Ok(orden);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    title: "Error al obtener la orden",
                    statusCode: 500
                );
            }
        })
        .WithName("ObtenerOrden")
        .WithSummary("Obtiene una orden por ID");

        // GET /ordenes/usuario/{usuarioId}
        group.MapGet("/usuario/{usuarioId:int}", async (int usuarioId, OrdenService service) =>
        {
            if (usuarioId <= 0)
                return Results.BadRequest(new { error = "El usuarioId debe ser mayor a 0" });

            try
            {
                var ordenes = await service.ObtenerOrdenesPorUsuarioAsync(usuarioId);
                return Results.Ok(ordenes);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    title: "Error al obtener las órdenes del usuario",
                    statusCode: 500
                );
            }
        })
        .WithName("ObtenerOrdenesPorUsuario")
        .WithSummary("Obtiene todas las órdenes de un usuario");
    }
}