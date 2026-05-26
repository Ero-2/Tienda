namespace MsPagos.Models;

public class TarjetaRequest
{
    public string NumeroTarjeta { get; set; } = string.Empty;
    public string NombreTitular { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Anio { get; set; }
    public string Cvv { get; set; } = string.Empty;
}
