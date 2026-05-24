using Microsoft.EntityFrameworkCore;
using Promociones.API.Models;

namespace Promociones.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Promocion> Promociones { get; set; }
    }
}