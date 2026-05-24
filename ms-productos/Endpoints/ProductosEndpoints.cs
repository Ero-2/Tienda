using MsProductos.Services;

namespace MsProductos.Endpoints;

public static class ProductosEndpoints
{
    public static void MapProductosEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/productos").WithTags("Productos");

        group.MapGet("/", async (
            string?        categoria,
            string?        q,
            int            limit   = 50,
            int            skip    = 0,
            ProductoService service = default!) =>
        {
            if (limit > 100) limit = 100;
            var result = await service.GetProductosAsync(categoria, q, limit, skip);
            return Results.Ok(result);
        })
        .WithName("GetProductos")
        .WithSummary("Lista productos con filtros opcionales");

        group.MapGet("/categorias", async (ProductoService service) =>
            Results.Ok(await service.GetCategoriasAsync()))
        .WithName("GetCategorias")
        .WithSummary("Lista todas las categorías disponibles");

        group.MapGet("/{id:int}", async (int id, ProductoService service) =>
        {
            var producto = await service.GetByIdAsync(id);
            return producto is null
                ? Results.NotFound(new { error = $"Producto {id} no encontrado" })
                : Results.Ok(producto);
        })
        .WithName("GetProductoById")
        .WithSummary("Obtiene un producto por ID");
    }
}
