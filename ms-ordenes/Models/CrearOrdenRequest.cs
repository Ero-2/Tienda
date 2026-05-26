namespace MsOrdenes.Models;

public class CrearOrdenRequest
{
    public int UsuarioId { get; set; }
    public string ModalidadPago { get; set; } = "contado";
    public List<ItemRequest> Items { get; set; } = new();
}

public class ItemRequest
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public bool EsElectronico { get; set; }
}