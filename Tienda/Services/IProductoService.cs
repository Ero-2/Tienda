using Tienda.Models;

namespace Tienda.Services;

public interface IProductoService
{
    Task<List<Product>> GetProductosAsync(string? categoria = null, string? q = null);
    Task<List<string>> GetCategoriasAsync();
}

public class ProductoDto
{
    public int     Id          { get; set; }
    public string  Nombre      { get; set; } = string.Empty;
    public string  Marca       { get; set; } = string.Empty;
    public string  Categoria   { get; set; } = string.Empty;
    public string  Descripcion { get; set; } = string.Empty;
    public decimal Precio      { get; set; }
    public string  ImagenUrl   { get; set; } = string.Empty;
    public int     Stock       { get; set; }
    public bool    EsNuevo     { get; set; }
    public bool    EsDestacado { get; set; }
    public double  Rating      { get; set; }
}

public class ProductosPagedDto
{
    public List<ProductoDto> Productos { get; set; } = new();
    public int Total { get; set; }
}
