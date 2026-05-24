using MsPagos.Services;

namespace MsPagos.Endpoints;

public static class PagosEndpoints
{
    public static void MapPagosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pagos")
            .WithTags("Pagos");

        // GET /pagos/orden/{ordenId}
        group.MapGet("/orden/{ordenId:int}", async (int ordenId, PagoService service) =>
        {
            if (ordenId <= 0)
                return Results.BadRequest(new { error = "El ordenId debe ser mayor a 0" });

            try
            {
                var pagos = await service.ObtenerPagosPorOrdenAsync(ordenId);

                if (pagos is null || pagos.Count == 0)
                    return Results.NotFound(new { error = $"No se encontraron pagos para la orden {ordenId}" });

                return Results.Ok(pagos);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    title: "Error al obtener los pagos",
                    statusCode: 500
                );
            }
        })
        .WithName("ObtenerPagosPorOrden")
        .WithSummary("Obtiene los pagos de una orden");
    }
}