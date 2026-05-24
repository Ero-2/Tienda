namespace ApiGateway.Contracts.Events;

/// <summary>
/// Publicado por: MS Catálogo
/// Consumido por: MS Órdenes
/// Se emite cuando cambia la existencia de un producto
/// (compra confirmada, cancelación, ajuste de inventario).
/// </summary>
public record StockActualizadoEvent
{
    public required string  ProductoId   { get; init; }
    public required int     Existencia   { get; init; }  // Cantidad actual en inventario
    public required string  CategoriaId  { get; init; }

    public DateTime OcurridoEn   { get; init; } = DateTime.UtcNow;
    public string?  CorrelationId { get; init; }
}
