using Microsoft.EntityFrameworkCore;
using MsOrdenes.Data;
using MsOrdenes.Endpoints;
using MsOrdenes.Handlers;
using MsOrdenes.Services;


var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server
builder.Services.AddDbContext<OrdenesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Servicios
builder.Services.AddScoped<OrdenService>();
builder.Services.AddHostedService<PagoConsumer>();
builder.Services.AddSingleton<RabbitMQPublisher>();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Migración automática con retry (SQL Server puede tardar en estar listo)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdenesDbContext>();
    for (int i = 0; i < 10; i++)
    {
        try { db.Database.Migrate(); break; }
        catch when (i < 9) { Thread.Sleep(3000); }
    }
}

app.MapGet("/", () => "MS Órdenes corriendo ✅");

// Endpoints
app.MapOrdenesEndpoints();

app.Run();