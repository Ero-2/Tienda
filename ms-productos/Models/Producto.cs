namespace MsProductos.Models;

public class Producto
{
    public int    Id          { get; set; }
    public string Nombre      { get; set; } = string.Empty;
    public string Marca       { get; set; } = string.Empty;
    public string Categoria   { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio     { get; set; }
    public string ImagenUrl   { get; set; } = string.Empty;
    public int    Stock       { get; set; }
    public bool   EsNuevo     { get; set; }
    public bool   EsDestacado { get; set; }
    public double Rating      { get; set; }
}

public class ProductoResponse
{
    public int    Id          { get; set; }
    public string Nombre      { get; set; } = string.Empty;
    public string Marca       { get; set; } = string.Empty;
    public string Categoria   { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio     { get; set; }
    public string ImagenUrl   { get; set; } = string.Empty;
    public int    Stock       { get; set; }
    public bool   EsNuevo     { get; set; }
    public bool   EsDestacado { get; set; }
    public double Rating      { get; set; }
}

public class ProductosPagedResponse
{
    public List<ProductoResponse> Productos { get; set; } = new();
    public int Total { get; set; }
    public int Skip  { get; set; }
    public int Limit { get; set; }
}
