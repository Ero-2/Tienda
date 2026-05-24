namespace MsPagos.Models;

public class PagoResponse
{
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public int MesesMsi { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? IdTransaccionOpenpay { get; set; }
    public string? ReferenciaOpenpay { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? ProcesadoEn { get; set; }
}