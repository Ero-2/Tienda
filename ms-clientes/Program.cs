using Microsoft.EntityFrameworkCore;
using MsClientes.Data;
using MsClientes.Endpoints;
using MsClientes.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ClientesDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServer"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<ClienteService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClientesDbContext>();
    try { db.Database.EnsureCreated(); }
    catch (Exception ex) when (ex.Message.Contains("already exists")) { }

    // Agregar columnas de crédito si no existen (para DBs existentes)
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Clientes ADD LimiteCredito DECIMAL(18,2) NOT NULL DEFAULT 0"); }
    catch { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Clientes ADD CreditoDisponible DECIMAL(18,2) NOT NULL DEFAULT 0"); }
    catch { }
}

app.MapGet("/", () => "MS Clientes corriendo ✅");
app.MapClientesEndpoints();

app.Run();
