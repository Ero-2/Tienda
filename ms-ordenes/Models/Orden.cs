namespace MsOrdenes.Models;

public class Orden
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = "pendiente"; // pendiente, confirmada, cancelada
    public decimal Subtotal { get; set; }
    public decimal DescuentoPct { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal Total { get; set; }
    public string ModalidadPago { get; set; } = "contado"; // contado, 3msi, 6msi, 12msi
    public int MesesMsi { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    // Navegación
    public List<ItemOrden> Items { get; set; } = new();
}