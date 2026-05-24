namespace ApiGateway.Contracts.Events;

/// <summary>
/// Publicado por: MS Envíos
/// Consumido por: MS Órdenes
/// Se emite cada vez que cambia el estado del envío
/// (pendiente → en camino → entregado / no entregado → reprogramado).
/// </summary>
public record EnvioActualizadoEvent
{
    public required string        OrdenId        { get; init; }
    public required string        NumRastreo     { get; init; }
    public required EstadoEnvio   Estado         { get; init; }

    /// <summary>Fecha estimada de entrega en UTC. Puede cambiar si se reprograma.</summary>
    public required DateTime FechaEstimada { get; init; }

    public DateTime OcurridoEn   { get; init; } = DateTime.UtcNow;
    public string?  CorrelationId { get; init; }

    /// <summary>Nota opcional del repartidor (ej. "Cliente no encontrado, se reprograma").</summary>
    public string? Nota { get; init; }
}

/// <summary>Estados posibles del ciclo de vida de un envío.</summary>
public enum EstadoEnvio
{
    Pendiente,
    Preparando,
    EnCamino,
    Entregado,
    NoEntregado,
    Reprogramado,
    Cancelado
}
