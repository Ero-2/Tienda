using Microsoft.EntityFrameworkCore;
using MsPagos.Models;

namespace MsPagos.Data;

public class PagosDbContext : DbContext
{
    public PagosDbContext(DbContextOptions<PagosDbContext> options)
        : base(options) { }

    public DbSet<Pago>     Pagos     => Set<Pago>();
    public DbSet<PlanPago> PlanPagos => Set<PlanPago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("pagos", "pagos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.Monto).HasColumnType("decimal(12,2)");
            entity.Property(e => e.TokenCheckout).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.TokenCheckout);
            entity.Property(e => e.IdTransaccionOpenpay).HasMaxLength(100);
            entity.Property(e => e.ReferenciaOpenpay).HasMaxLength(200);
            entity.Property(e => e.RespuestaOpenpay).HasColumnType("text");
            entity.Property(e => e.TarjetaEnmascarada).HasMaxLength(25);
            entity.Property(e => e.MarcaTarjeta).HasMaxLength(20);
        });

        modelBuilder.Entity<PlanPago>(entity =>
        {
            entity.ToTable("plan_pagos", "pagos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.TotalFinanciado).HasColumnType("decimal(12,2)");
            entity.Property(e => e.CuotaMensual).HasColumnType("decimal(12,2)");
        });
    }
}