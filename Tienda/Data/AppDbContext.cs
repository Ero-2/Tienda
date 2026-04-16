using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tienda.Models;

namespace Tienda.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();

        public AppDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Ruta segura para guardar la DB en el celular
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "eromode_tienda.db");
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }
    }
}
