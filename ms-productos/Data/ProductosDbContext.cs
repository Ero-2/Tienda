using Microsoft.EntityFrameworkCore;
using MsProductos.Models;

namespace MsProductos.Data;

public class ProductosDbContext : DbContext
{
    public ProductosDbContext(DbContextOptions<ProductosDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
}
