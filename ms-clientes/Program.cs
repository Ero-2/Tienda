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
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "MS Clientes corriendo ✅");
app.MapClientesEndpoints();

app.Run();
