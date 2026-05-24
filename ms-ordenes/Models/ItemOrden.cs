namespace MsOrdenes.Models;

public class ItemOrden
{
    public int Id { get; set; }
    public int OrdenId { get; set; }
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public bool EsElectronico { get; set; }

    // Navegación
    public Orden Orden { get; set; } = null!;
}