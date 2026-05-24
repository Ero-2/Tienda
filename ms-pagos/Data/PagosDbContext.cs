using Microsoft.EntityFrameworkCore;
using MsPagos.Models;

namespace MsPagos.Data;

public class PagosDbContext : DbContext
{
    public PagosDbContext(DbContextOptions<PagosDbContext> options)
        : base(options) { }

    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("pagos", "pagos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.Monto).HasColumnType("decimal(12,2)");
            entity.Property(e => e.IdTransaccionOpenpay).HasMaxLength(100);
            entity.Property(e => e.ReferenciaOpenpay).HasMaxLength(200);
            entity.Property(e => e.RespuestaOpenpay).HasColumnType("text");
        });
    }
}