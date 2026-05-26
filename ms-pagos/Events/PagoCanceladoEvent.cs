namespace MsPagos.Events;

public class PagoCanceladoEvent
{
    public int OrdenId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime ProcesadoEn { get; set; } = DateTime.UtcNow;
}
