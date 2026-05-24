namespace ApiGateway.Contracts.Events;

/// <summary>
/// Publicado por: MS Pagos
/// Consumido por: MS Órdenes
/// Se emite cuando OpenPay rechaza la transacción o el crédito es insuficiente.
/// </summary>
public record PagoRechazadoEvent
{
    public required string OrdenId       { get; init; }
    public required string ErrorCodigo   { get; init; }  // Código de error de OpenPay / interno
    public required string ErrorMensaje  { get; init; }  // Descripción legible del error

    public DateTime OcurridoEn   { get; init; } = DateTime.UtcNow;
    public string?  CorrelationId { get; init; }
}
