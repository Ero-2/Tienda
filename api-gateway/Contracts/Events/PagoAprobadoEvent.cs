namespace ApiGateway.Contracts.Events;

/// <summary>
/// Publicado por: MS Pagos
/// Consumido por: MS Órdenes, MS Envíos
/// Se emite cuando OpenPay o el crédito interno confirman la transacción.
/// </summary>
public record PagoAprobadoEvent
{
    public required string  OrdenId        { get; init; }
    public required string  TransaccionId  { get; init; }  // ID de OpenPay o crédito interno
    public required decimal Monto          { get; init; }

    /// <summary>
    /// Meses sin intereses aplicados (0 = pago de contado).
    /// Válido solo cuando el pago se realizó con tarjeta vía OpenPay.
    /// </summary>
    public int MsiMeses { get; init; } = 0;

    public DateTime OcurridoEn  { get; init; } = DateTime.UtcNow;
    public string?  CorrelationId { get; init; }
}
