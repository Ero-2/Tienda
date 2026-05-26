namespace MsPagos.Models;

public class ConfirmacionPagoResponse
{
    public string Token { get; set; } = string.Empty;
    public int OrdenId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? TransaccionId { get; set; }
    public string? Referencia { get; set; }
    public string? TarjetaEnmascarada { get; set; }
    public string? MarcaTarjeta { get; set; }
    public bool YaProcesado { get; set; }
    public DateTime? ProcesadoEn { get; set; }
}
