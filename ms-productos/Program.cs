using Microsoft.EntityFrameworkCore;
using MsProductos.Data;
using MsProductos.Endpoints;
using MsProductos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductosDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServer"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

builder.Services.AddHttpClient<DummyJsonSeeder>();
builder.Services.AddScoped<ProductoService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<ProductosDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DummyJsonSeeder>();

    for (int i = 0; i < 10; i++)
    {
        try { db.Database.EnsureCreated(); break; }
        catch when (i < 9) { Thread.Sleep(3000); }
    }
    await seeder.SeedAsync(db);
}

app.MapGet("/", () => "MS Productos corriendo ✅");
app.MapProductosEndpoints();

app.Run();
