namespace ApiGateway.Contracts.Events;

/// <summary>
/// Publicado por: MS Órdenes
/// Consumido por: MS Pagos
/// Se emite cuando un cliente confirma su carrito y se genera una orden.
/// </summary>
public record OrdenCreadaEvent
{
    public required string   OrdenId      { get; init; }
    public required string   ClienteId    { get; init; }
    public required decimal  Total        { get; init; }
    public          decimal  Descuento    { get; init; } = 0;
    public required ItemDto[] Items       { get; init; }

    /// <summary>Fecha UTC en que se emitió el evento.</summary>
    public DateTime OcurridoEn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Trazabilidad EDA: mismo ID que el X-Correlation-Id del request HTTP original.
    /// </summary>
    public string? CorrelationId { get; init; }
}

public record ItemDto
{
    public required string  ProductoId  { get; init; }
    public required string  Nombre      { get; init; }
    public required int     Cantidad    { get; init; }
    public required decimal PrecioUnitario { get; init; }
    public required string  Categoria   { get; init; }  // "electronica" aplica 5% descuento
}
