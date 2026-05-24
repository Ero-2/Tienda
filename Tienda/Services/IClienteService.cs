using Tienda.Models;

namespace Tienda.Services;

public interface IClienteService
{
    Task<List<Address>> GetDireccionesAsync(int clienteId);
    Task<Address?>      AgregarDireccionAsync(int clienteId, Address address);
    Task<bool>          EliminarDireccionAsync(int clienteId, int direccionId);
}

// DTOs que devuelve ms-clientes
public class DireccionDto
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

public class LoginResponseDto
{
    public string Token     { get; set; } = string.Empty;
    public int    ClienteId { get; set; }
    public string Nombre    { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
}
