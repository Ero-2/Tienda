using MsClientes.Models;
using MsClientes.Services;

namespace MsClientes.Endpoints;

public static class ClientesEndpoints
{
    public static void MapClientesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clientes").WithTags("Clientes");

        // POST /api/clientes/registro
        group.MapPost("/registro", async (RegistroRequest req, ClienteService service) =>
        {
            if (string.IsNullOrWhiteSpace(req.Nombre) ||
                string.IsNullOrWhiteSpace(req.Email)  ||
                string.IsNullOrWhiteSpace(req.Password) ||
                req.Password.Length < 6)
                return Results.BadRequest(new { error = "Datos inválidos" });

            var (result, error) = await service.RegistrarAsync(req);
            return error is null
                ? Results.Ok(result)
                : Results.Conflict(new { error });
        })
        .WithName("Registro")
        .WithSummary("Registra un nuevo cliente");

        // POST /api/clientes/login
        group.MapPost("/login", async (LoginRequest req, ClienteService service) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest(new { error = "Email y contraseña requeridos" });

            var (result, error) = await service.LoginAsync(req);
            return error is null
                ? Results.Ok(result)
                : Results.Unauthorized();
        })
        .WithName("Login")
        .WithSummary("Autentica un cliente y retorna JWT");

        // GET /api/clientes/{id}
        group.MapGet("/{id:int}", async (int id, ClienteService service) =>
        {
            var perfil = await service.GetPerfilAsync(id);
            return perfil is null
                ? Results.NotFound(new { error = $"Cliente {id} no encontrado" })
                : Results.Ok(perfil);
        })
        .WithName("GetPerfil")
        .WithSummary("Obtiene el perfil de un cliente");

        // PUT /api/clientes/{id}
        group.MapPut("/{id:int}", async (int id, ActualizarClienteRequest req, ClienteService service) =>
        {
            var ok = await service.ActualizarPerfilAsync(id, req);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .WithName("ActualizarPerfil")
        .WithSummary("Actualiza nombre y/o teléfono del cliente");

        // GET /api/clientes/{id}/direcciones
        group.MapGet("/{id:int}/direcciones", async (int id, ClienteService service) =>
            Results.Ok(await service.GetDireccionesAsync(id)))
        .WithName("GetDirecciones")
        .WithSummary("Lista las direcciones del cliente");

        // POST /api/clientes/{id}/direcciones
        group.MapPost("/{id:int}/direcciones", async (int id, DireccionRequest req, ClienteService service) =>
        {
            var (dir, error) = await service.AgregarDireccionAsync(id, req);
            return error is null
                ? Results.Created($"/api/clientes/{id}/direcciones/{dir!.Id}", dir)
                : Results.BadRequest(new { error });
        })
        .WithName("AgregarDireccion")
        .WithSummary("Agrega una dirección al cliente");

        // DELETE /api/clientes/{id}/direcciones/{dirId}
        group.MapDelete("/{id:int}/direcciones/{dirId:int}", async (int id, int dirId, ClienteService service) =>
        {
            var ok = await service.EliminarDireccionAsync(id, dirId);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .WithName("EliminarDireccion")
        .WithSummary("Elimina una dirección del cliente");

        // GET /api/clientes/{id}/credito
        group.MapGet("/{id:int}/credito", async (int id, ClienteService service) =>
        {
            var credito = await service.GetCreditoAsync(id);
            return credito is null ? Results.NotFound() : Results.Ok(credito);
        })
        .WithName("GetCredito")
        .WithSummary("Obtiene el crédito disponible del cliente");

        // POST /api/clientes/{id}/credito/debitar
        group.MapPost("/{id:int}/credito/debitar", async (int id, DebitarCreditoRequest req, ClienteService service) =>
        {
            var (result, error) = await service.DebitarCreditoAsync(id, req.Monto);
            return error is null ? Results.Ok(result) : Results.BadRequest(new { error });
        })
        .WithName("DebitarCredito")
        .WithSummary("Debita monto del crédito del cliente");
    }
}
