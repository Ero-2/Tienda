namespace MsPagos.Models;

public class CheckoutResponse
{
    public string Token { get; set; } = string.Empty;
    public int OrdenId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "MXN";
    public int MesesMsi { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string UrlPago { get; set; } = string.Empty;
}
