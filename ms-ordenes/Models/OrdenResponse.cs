namespace MsOrdenes.Models;

public class OrdenResponse
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DescuentoPct { get; set; }
    public decimal DescuentoMonto { get; set; }
    public decimal Total { get; set; }
    public string ModalidadPago { get; set; } = string.Empty;
    public int MesesMsi { get; set; }
    public DateTime CreadoEn { get; set; }
    public List<ItemOrdenResponse> Items { get; set; } = new();
}

public class ItemOrdenResponse
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public bool EsElectronico { get; set; }
}