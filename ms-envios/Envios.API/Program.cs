using Envios.API.Data;
using Microsoft.EntityFrameworkCore;
using Envios.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Configurar la Base de Datos con Resiliencia
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ¡AQUÍ ESTABA EL ERROR! Lo correcto es builder.Build();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

EnviosEndpoints.Map(app);

app.Run();