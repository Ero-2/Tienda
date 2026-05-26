namespace MsOrdenes.Events;

public class OrdenCreadaEvent
{
    public int OrdenId { get; set; }
    public int ClienteId { get; set; }
    public decimal Total { get; set; }
    public decimal Descuento { get; set; }
    public string ModalidadPago { get; set; } = string.Empty;
    public int MesesMsi { get; set; }
    public List<ItemEvent> Items { get; set; } = new();
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public class ItemEvent
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public bool EsElectronico { get; set; }
}