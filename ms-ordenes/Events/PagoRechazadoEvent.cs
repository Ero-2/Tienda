namespace MsOrdenes.Events;

public class PagoRechazadoEvent
{
    public int OrdenId { get; set; }
    public string ErrorCodigo { get; set; } = string.Empty;
    public string ErrorMensaje { get; set; } = string.Empty;
    public DateTime ProcesadoEn { get; set; }
}