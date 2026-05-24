namespace Tienda.Models;

public class Promocion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal PorcentajeDescuento { get; set; }
    public decimal MontoMinimo { get; set; }
    public bool Activa { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}

public class DescuentoResponse
{
    public decimal TotalOriginal { get; set; }
    public decimal DescuentoAplicado { get; set; }
    public decimal TotalAPagar { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class SolicitudDescuento
{
    public decimal Total { get; set; }
    public bool EsSoloElectronica { get; set; }
}
