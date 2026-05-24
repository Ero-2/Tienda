using Envios.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Envios.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Envio> Envios { get; set; }
        public DbSet<HistorialEstado> HistorialEstados { get; set; }
    }
}