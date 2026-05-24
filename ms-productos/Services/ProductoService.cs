using Microsoft.EntityFrameworkCore;
using MsProductos.Data;
using MsProductos.Models;

namespace MsProductos.Services;

public class ProductoService
{
    private readonly ProductosDbContext _db;

    public ProductoService(ProductosDbContext db) => _db = db;

    public async Task<ProductosPagedResponse> GetProductosAsync(
        string? categoria, string? q, int limit, int skip)
    {
        var query = _db.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(p => p.Categoria == categoria);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p =>
                p.Nombre.Contains(q) ||
                p.Marca.Contains(q)  ||
                p.Descripcion.Contains(q));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(limit)
            .Select(p => new ProductoResponse
            {
                Id          = p.Id,
                Nombre      = p.Nombre,
                Marca       = p.Marca,
                Categoria   = p.Categoria,
                Descripcion = p.Descripcion,
                Precio      = p.Precio,
                ImagenUrl   = p.ImagenUrl,
                Stock       = p.Stock,
                EsNuevo     = p.EsNuevo,
                EsDestacado = p.EsDestacado,
                Rating      = p.Rating
            })
            .ToListAsync();

        return new ProductosPagedResponse
        {
            Productos = items,
            Total     = total,
            Skip      = skip,
            Limit     = limit
        };
    }

    public async Task<ProductoResponse?> GetByIdAsync(int id)
    {
        var p = await _db.Productos.FindAsync(id);
        if (p is null) return null;

        return new ProductoResponse
        {
            Id          = p.Id,
            Nombre      = p.Nombre,
            Marca       = p.Marca,
            Categoria   = p.Categoria,
            Descripcion = p.Descripcion,
            Precio      = p.Precio,
            ImagenUrl   = p.ImagenUrl,
            Stock       = p.Stock,
            EsNuevo     = p.EsNuevo,
            EsDestacado = p.EsDestacado,
            Rating      = p.Rating
        };
    }

    public async Task<List<string>> GetCategoriasAsync() =>
        await _db.Productos
            .Select(p => p.Categoria)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
}
