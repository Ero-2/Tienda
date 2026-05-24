namespace MsPagos.Models;

public class Pago
{
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public int MesesMsi { get; set; }
    public string Estado { get; set; } = "pendiente"; // pendiente, aprobado, rechazado
    public string? IdTransaccionOpenpay { get; set; }
    public string? ReferenciaOpenpay { get; set; }
    public string? RespuestaOpenpay { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime? ProcesadoEn { get; set; }
}