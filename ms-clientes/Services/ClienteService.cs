using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MsClientes.Data;
using MsClientes.Models;

namespace MsClientes.Services;

public class ClienteService
{
    private readonly ClientesDbContext _db;
    private readonly JwtService        _jwt;

    public ClienteService(ClientesDbContext db, JwtService jwt)
    {
        _db  = db;
        _jwt = jwt;
    }

    // ── Auth ──────────────────────────────────────────────────

    private static readonly decimal[] _creditOptions = [200m, 500m, 1000m];
    private static readonly Random    _rng            = new();

    public async Task<(LoginResponse? result, string? error)> RegistrarAsync(RegistroRequest req)
    {
        if (await _db.Clientes.AnyAsync(c => c.Email == req.Email))
            return (null, "El email ya está registrado");

        var limite  = _creditOptions[_rng.Next(_creditOptions.Length)];
        var hasher  = new PasswordHasher<string>();
        var cliente = new Cliente
        {
            Nombre             = req.Nombre,
            Email              = req.Email,
            PasswordHash       = hasher.HashPassword(req.Email, req.Password),
            Telefono           = req.Telefono,
            CreadoEn           = DateTime.UtcNow,
            LimiteCredito      = limite,
            CreditoDisponible  = limite
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerarToken(cliente.Id, cliente.Email);
        return (new LoginResponse
        {
            Token     = token,
            ClienteId = cliente.Id,
            Nombre    = cliente.Nombre,
            Email     = cliente.Email
        }, null);
    }

    public async Task<(LoginResponse? result, string? error)> LoginAsync(LoginRequest req)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email == req.Email);
        if (cliente is null)
            return (null, "Email o contraseña incorrectos");

        var hasher = new PasswordHasher<string>();
        var verify = hasher.VerifyHashedPassword(req.Email, cliente.PasswordHash, req.Password);

        if (verify == PasswordVerificationResult.Failed)
            return (null, "Email o contraseña incorrectos");

        var token = _jwt.GenerarToken(cliente.Id, cliente.Email);
        return (new LoginResponse
        {
            Token     = token,
            ClienteId = cliente.Id,
            Nombre    = cliente.Nombre,
            Email     = cliente.Email
        }, null);
    }

    // ── Perfil ────────────────────────────────────────────────

    public async Task<ClienteResponse?> GetPerfilAsync(int id)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return null;

        return new ClienteResponse
        {
            Id                = c.Id,
            Nombre            = c.Nombre,
            Email             = c.Email,
            Telefono          = c.Telefono,
            CreadoEn          = c.CreadoEn,
            LimiteCredito     = c.LimiteCredito,
            CreditoDisponible = c.CreditoDisponible
        };
    }

    public async Task<CreditoResponse?> GetCreditoAsync(int id)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return null;
        return new CreditoResponse { LimiteCredito = c.LimiteCredito, CreditoDisponible = c.CreditoDisponible };
    }

    public async Task<(CreditoResponse? result, string? error)> DebitarCreditoAsync(int id, decimal monto)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return (null, "Cliente no encontrado");
        if (c.CreditoDisponible < monto)
            return (null, $"Crédito insuficiente. Disponible: ${c.CreditoDisponible:F2}");

        c.CreditoDisponible -= monto;
        await _db.SaveChangesAsync();
        return (new CreditoResponse { LimiteCredito = c.LimiteCredito, CreditoDisponible = c.CreditoDisponible }, null);
    }

    public async Task<bool> ActualizarPerfilAsync(int id, ActualizarClienteRequest req)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return false;

        if (req.Nombre   is not null) c.Nombre   = req.Nombre;
        if (req.Telefono is not null) c.Telefono = req.Telefono;

        await _db.SaveChangesAsync();
        return true;
    }

    // ── Direcciones ───────────────────────────────────────────

    public async Task<List<DireccionResponse>> GetDireccionesAsync(int clienteId)
    {
        return await _db.Direcciones
            .Where(d => d.ClienteId == clienteId)
            .Select(d => new DireccionResponse
            {
                Id           = d.Id,
                Nombre       = d.Nombre,
                Calle        = d.Calle,
                Ciudad       = d.Ciudad,
                Estado       = d.Estado,
                CodigoPostal = d.CodigoPostal,
                Pais         = d.Pais,
                EsPrincipal  = d.EsPrincipal
            })
            .ToListAsync();
    }

    public async Task<(DireccionResponse? dir, string? error)> AgregarDireccionAsync(int clienteId, DireccionRequest req)
    {
        if (!await _db.Clientes.AnyAsync(c => c.Id == clienteId))
            return (null, "Cliente no encontrado. Inicia sesión con una cuenta para guardar direcciones.");

        if (req.EsPrincipal)
        {
            var anteriores = await _db.Direcciones
                .Where(d => d.ClienteId == clienteId && d.EsPrincipal)
                .ToListAsync();
            anteriores.ForEach(d => d.EsPrincipal = false);
        }

        var dir = new Direccion
        {
            ClienteId    = clienteId,
            Nombre       = req.Nombre,
            Calle        = req.Calle,
            Ciudad       = req.Ciudad,
            Estado       = req.Estado,
            CodigoPostal = req.CodigoPostal,
            Pais         = req.Pais,
            EsPrincipal  = req.EsPrincipal
        };

        _db.Direcciones.Add(dir);
        await _db.SaveChangesAsync();

        return (new DireccionResponse
        {
            Id           = dir.Id,
            Nombre       = dir.Nombre,
            Calle        = dir.Calle,
            Ciudad       = dir.Ciudad,
            Estado       = dir.Estado,
            CodigoPostal = dir.CodigoPostal,
            Pais         = dir.Pais,
            EsPrincipal  = dir.EsPrincipal
        }, null);
    }

    public async Task<bool> EliminarDireccionAsync(int clienteId, int direccionId)
    {
        var dir = await _db.Direcciones
            .FirstOrDefaultAsync(d => d.Id == direccionId && d.ClienteId == clienteId);

        if (dir is null) return false;

        _db.Direcciones.Remove(dir);
        await _db.SaveChangesAsync();
        return true;
    }
}
