namespace Tienda.Models;

public class Envio
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string NumRastreo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Preparando";
    public string DireccionEntrega { get; set; } = string.Empty;
    public DateTime FechaEstimada { get; set; }
}

public class HistorialEstadoEnvio
{
    public int Id { get; set; }
    public int EnvioId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistrado { get; set; }
}

public class EnvioRastreoResponse
{
    public Envio Envio { get; set; } = new();
    public List<HistorialEstadoEnvio> Historial { get; set; } = new();
}
