using System.Text.Json;
using Tienda.Models;

namespace Tienda.Services;

public class ProductoService : IProductoService
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductoService(HttpClient http) => _http = http;

    public async Task<List<Product>> GetProductosAsync(
        string? categoria = null, string? q = null)
    {
        var url = "/api/productos?limit=100";

        if (!string.IsNullOrWhiteSpace(categoria) && categoria != "TODO")
            url += $"&categoria={Uri.EscapeDataString(categoria)}";

        if (!string.IsNullOrWhiteSpace(q))
            url += $"&q={Uri.EscapeDataString(q)}";

        try
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return [];

            var json   = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProductosPagedDto>(json, JsonOpts);
            return result?.Productos.Select(MapToProduct).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<string>> GetCategoriasAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/productos/categorias");
            if (!response.IsSuccessStatusCode) return [];
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(json, JsonOpts) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static Product MapToProduct(ProductoDto dto) => new()
    {
        Id          = dto.Id,
        Name        = dto.Nombre,
        Brand       = dto.Marca,
        Category    = dto.Categoria,
        Description = dto.Descripcion,
        Price       = dto.Precio,
        ImageUrl    = dto.ImagenUrl,
        Stock       = dto.Stock,
        IsNew       = dto.EsNuevo,
        IsHot       = dto.EsDestacado
    };
}
