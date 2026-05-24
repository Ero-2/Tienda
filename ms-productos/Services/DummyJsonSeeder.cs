using System.Text.Json;
using MsProductos.Data;
using MsProductos.Models;

namespace MsProductos.Services;

public class DummyJsonSeeder
{
    private readonly HttpClient _http;
    private readonly ILogger<DummyJsonSeeder> _logger;

    public DummyJsonSeeder(HttpClient http, ILogger<DummyJsonSeeder> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task SeedAsync(ProductosDbContext db)
    {
        if (db.Productos.Any()) return;

        _logger.LogInformation("Seeding products from DummyJSON...");

        var productos = new List<Producto>();
        int globalIndex = 0;

        foreach (var skip in new[] { 0, 100 })
        {
            try
            {
                var response = await _http.GetAsync(
                    $"https://dummyjson.com/products?limit=100&skip={skip}");

                if (!response.IsSuccessStatusCode) break;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var arr = doc.RootElement.GetProperty("products");

                foreach (var item in arr.EnumerateArray())
                {
                    var rating = item.GetProperty("rating").GetDouble();

                    productos.Add(new Producto
                    {
                        Nombre      = item.GetProperty("title").GetString()       ?? "",
                        Descripcion = item.GetProperty("description").GetString() ?? "",
                        Precio      = item.GetProperty("price").GetDecimal(),
                        Marca       = item.TryGetProperty("brand", out var b)
                                        ? b.GetString() ?? "" : "",
                        Categoria   = item.GetProperty("category").GetString()    ?? "",
                        ImagenUrl   = item.GetProperty("thumbnail").GetString()   ?? "",
                        Stock       = item.GetProperty("stock").GetInt32(),
                        Rating      = rating,
                        EsDestacado = rating >= 4.5,
                        EsNuevo     = globalIndex < 20
                    });

                    globalIndex++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching DummyJSON (skip={Skip})", skip);
                break;
            }
        }

        if (productos.Count > 0)
        {
            db.Productos.AddRange(productos);
            await db.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} products", productos.Count);
        }
    }
}
