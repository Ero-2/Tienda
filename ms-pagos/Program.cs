using Microsoft.EntityFrameworkCore;
using MsPagos.Data;
using MsPagos.Endpoints;
using MsPagos.Handlers;
using MsPagos.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server
builder.Services.AddDbContext<PagosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));
    
// Servicios
builder.Services.AddScoped<PagoService>();
builder.Services.AddSingleton<OpenPayService>();
builder.Services.AddSingleton<RabbitMQPublisher>();

// Consumer RabbitMQ
builder.Services.AddHostedService<OrdenCreadaConsumer>();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Migración automática con retry
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PagosDbContext>();
    for (int i = 0; i < 10; i++)
    {
        try { db.Database.Migrate(); break; }
        catch when (i < 9) { Thread.Sleep(3000); }
    }
}

app.MapGet("/", () => "MS Pagos corriendo ✅");

app.MapPagosEndpoints();

app.Run();