namespace MsOrdenes.Events;

public class PagoAprobadoEvent
{
    public int OrdenId { get; set; }
    public string TransaccionId { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public int MesesMsi { get; set; }
    public DateTime ProcesadoEn { get; set; }
}