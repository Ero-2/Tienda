namespace MsClientes.Models;

public class Cliente
{
    public int     Id                 { get; set; }
    public string  Nombre             { get; set; } = string.Empty;
    public string  Email              { get; set; } = string.Empty;
    public string  PasswordHash       { get; set; } = string.Empty;
    public string? Telefono           { get; set; }
    public DateTime CreadoEn         { get; set; } = DateTime.UtcNow;
    public decimal LimiteCredito      { get; set; }
    public decimal CreditoDisponible  { get; set; }
    public List<Direccion> Direcciones { get; set; } = new();
}

public class Direccion
{
    public int    Id           { get; set; }
    public int    ClienteId    { get; set; }
    public string Nombre       { get; set; } = string.Empty; // "Casa", "Trabajo", etc.
    public string Calle        { get; set; } = string.Empty;
    public string Ciudad       { get; set; } = string.Empty;
    public string Estado       { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Pais         { get; set; } = "México";
    public bool   EsPrincipal  { get; set; }
    public Cliente Cliente     { get; set; } = null!;
}

// ── Request DTOs ──────────────────────────────────────────────
public class RegistroRequest
{
    public string  Nombre   { get; set; } = string.Empty;
    public string  Email    { get; set; } = string.Empty;
    public string  Password { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

public class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ActualizarClienteRequest
{
    public string? Nombre   { get; set; }
    public string? Telefono { get; set; }
}

public class DireccionRequest
{
    public string Nombre       { get; set; } = string.Empty;
    public string Calle        { get; set; } = string.Empty;
    public string Ciudad       { get; set; } = string.Empty;
    public string Estado       { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Pais         { get; set; } = "México";
    public bool   EsPrincipal  { get; set; }
}

// ── Response DTOs ─────────────────────────────────────────────
public class LoginResponse
{
    public string Token     { get; set; } = string.Empty;
    public int    ClienteId { get; set; }
    public string Nombre    { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
}

public class ClienteResponse
{
    public int     Id                { get; set; }
    public string  Nombre            { get; set; } = string.Empty;
    public string  Email             { get; set; } = string.Empty;
    public string? Telefono          { get; set; }
    public DateTime CreadoEn        { get; set; }
    public decimal LimiteCredito     { get; set; }
    public decimal CreditoDisponible { get; set; }
}

public class CreditoResponse
{
    public decimal LimiteCredito     { get; set; }
    public decimal CreditoDisponible { get; set; }
    public decimal CreditoUsado      => LimiteCredito - CreditoDisponible;
}

public class DebitarCreditoRequest
{
    public decimal Monto { get; set; }
}

public class DireccionResponse
{
    public int    Id           { get; set; }
    public string Nombre       { get; set; } = string.Empty;
    public string Calle        { get; set; } = string.Empty;
    public string Ciudad       { get; set; } = string.Empty;
    public string Estado       { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Pais         { get; set; } = string.Empty;
    public bool   EsPrincipal  { get; set; }
}
