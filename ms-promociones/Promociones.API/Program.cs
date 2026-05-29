using Microsoft.EntityFrameworkCore;
using Promociones.API.Data;
using Promociones.API.Endpoints;
using Promociones.API.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar PostgreSQL con Resiliencia (Requisito del Maestro)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptionsAction: sqlOptions =>
        {
            // Resiliencia activa para Postgres
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Crear el esquema en Postgres si no existe (no hay migraciones EF en este servicio)
// y sembrar promociones de ejemplo para que la pantalla tenga datos.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Promociones.Any())
    {
        db.Promociones.AddRange(
            new Promocion { Nombre = "5% en Electrónicos", Tipo = "Electronicos", PorcentajeDescuento = 5, MontoMinimo = 0, Activa = true },
            new Promocion { Nombre = "10% en compras ≥ $1,000", Tipo = "General", PorcentajeDescuento = 10, MontoMinimo = 1000, Activa = true },
            new Promocion { Nombre = "Envío gratis ≥ $50", Tipo = "Envio", PorcentajeDescuento = 0, MontoMinimo = 50, Activa = true });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Conectar los Endpoints
PromocionesEndpoints.Map(app);

app.Run();