using Microsoft.EntityFrameworkCore;
using MsClientes.Models;

namespace MsClientes.Data;

public class ClientesDbContext : DbContext
{
    public ClientesDbContext(DbContextOptions<ClientesDbContext> options) : base(options) { }

    public DbSet<Cliente>   Clientes   => Set<Cliente>();
    public DbSet<Direccion> Direcciones => Set<Direccion>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        model.Entity<Direccion>()
            .HasOne(d => d.Cliente)
            .WithMany(c => c.Direcciones)
            .HasForeignKey(d => d.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
