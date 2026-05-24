using Microsoft.EntityFrameworkCore;
using Promociones.API.Data;
using Promociones.API.Endpoints;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Conectar los Endpoints
PromocionesEndpoints.Map(app);

app.Run();