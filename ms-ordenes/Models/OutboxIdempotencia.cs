namespace MsOrdenes.Models;

public class OutboxIdempotencia
{
    public int Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}