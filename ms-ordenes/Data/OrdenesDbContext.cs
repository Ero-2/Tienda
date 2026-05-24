using Microsoft.EntityFrameworkCore;
using MsOrdenes.Models;

namespace MsOrdenes.Data;

public class OrdenesDbContext : DbContext
{
    public OrdenesDbContext(DbContextOptions<OrdenesDbContext> options) 
        : base(options) { }

    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<ItemOrden> ItemsOrden => Set<ItemOrden>();
    public DbSet<OutboxIdempotencia> Idempotencias => Set<OutboxIdempotencia>(); // ← NUEVO

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tabla Ordenes
        modelBuilder.Entity<Orden>(entity =>
        {
            entity.ToTable("ordenes", "ordenes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.ModalidadPago).HasMaxLength(30);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(12,2)");
            entity.Property(e => e.DescuentoPct).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DescuentoMonto).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(12,2)");
        });

        // Tabla ItemsOrden
        modelBuilder.Entity<ItemOrden>(entity =>
        {
            entity.ToTable("items_orden", "ordenes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreProducto).HasMaxLength(200);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(12,2)");

            entity.HasOne(e => e.Orden)
                  .WithMany(o => o.Items)
                  .HasForeignKey(e => e.OrdenId);
        });

        // Tabla Idempotencias ← NUEVO
        modelBuilder.Entity<OutboxIdempotencia>(entity =>
        {
            entity.ToTable("idempotencias", "ordenes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
        });
    }
}