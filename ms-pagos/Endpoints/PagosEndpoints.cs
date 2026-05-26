using MsPagos.Models;
using MsPagos.Services;

namespace MsPagos.Endpoints;

public static class PagosEndpoints
{
    public static void MapPagosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pagos").WithTags("Pagos");

        group.MapGet("/orden/{ordenId:int}", async (int ordenId, PagoService service) =>
        {
            if (ordenId <= 0)
                return Results.BadRequest(new { error = "El ordenId debe ser mayor a 0" });

            try
            {
                var pagos = await service.ObtenerPagosPorOrdenAsync(ordenId);
                return pagos is null || pagos.Count == 0
                    ? Results.NotFound(new { error = $"No se encontraron pagos para la orden {ordenId}" })
                    : Results.Ok(pagos);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "Error al obtener los pagos", statusCode: 500);
            }
        })
        .WithName("ObtenerPagosPorOrden")
        .WithSummary("Obtiene los pagos de una orden");

        group.MapGet("/checkout/{token}", async (string token, HttpContext http, PagoService service) =>
        {
            if (string.IsNullOrWhiteSpace(token))
                return Results.BadRequest(new { error = "El token del checkout es obligatorio" });

            try
            {
                var baseUrl  = $"{http.Request.Scheme}://{http.Request.Host}";
                var checkout = await service.ObtenerCheckoutPorTokenAsync(token, baseUrl);
                return checkout is null
                    ? Results.NotFound(new { error = "No se encontró un checkout con ese token" })
                    : Results.Ok(checkout);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "Error al obtener el checkout", statusCode: 500);
            }
        })
        .WithName("ObtenerCheckout")
        .WithSummary("Obtiene los datos del checkout para capturar la tarjeta");

        group.MapPost("/checkout/{token}/confirmar", async (string token, TarjetaRequest? tarjeta, PagoService service) =>
        {
            if (string.IsNullOrWhiteSpace(token))
                return Results.BadRequest(new { error = "El token del checkout es obligatorio" });

            if (tarjeta is null)
                return Results.BadRequest(new { error = "Se requieren los datos de la tarjeta" });

            var errores = TarjetaValidator.Validar(tarjeta);
            if (errores.Count > 0)
                return Results.BadRequest(new { error = "Datos de tarjeta inválidos", detalles = errores });

            try
            {
                var (resultado, error) = await service.ConfirmarPagoAsync(token, tarjeta);

                if (error == "NO_ENCONTRADO")
                    return Results.NotFound(new { error = "No se encontró un checkout con ese token" });

                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "Error al procesar el pago", statusCode: 500);
            }
        })
        .WithName("ConfirmarPago")
        .WithSummary("Captura la tarjeta y procesa el pago (aprobado/rechazado/cancelado)");

        // GET /pagos/planes/{usuarioId}
        group.MapGet("/planes/{usuarioId:int}", async (int usuarioId, PagoService service) =>
        {
            var planes = await service.ObtenerPlanesPorUsuarioAsync(usuarioId);
            return Results.Ok(planes);
        })
        .WithName("ObtenerPlanesMSI")
        .WithSummary("Obtiene los planes MSI activos del usuario");

        // POST /pagos/planes/{planId}/pagar
        group.MapPost("/planes/{planId:int}/pagar", async (int planId, PagarCuotaRequest req, PagoService service) =>
        {
            var errores = TarjetaValidator.Validar(req.Tarjeta);
            if (errores.Count > 0)
                return Results.BadRequest(new { error = "Datos de tarjeta inválidos", detalles = errores });

            var (resultado, error) = await service.PagarCuotaAsync(planId, req.UsuarioId, req.Tarjeta);
            return error is null ? Results.Ok(resultado) : Results.BadRequest(new { error });
        })
        .WithName("PagarCuotaMSI")
        .WithSummary("Paga la siguiente cuota de un plan MSI");
    }
}

public class PagarCuotaRequest
{
    public int           UsuarioId { get; set; }
    public TarjetaRequest Tarjeta  { get; set; } = new();
}
