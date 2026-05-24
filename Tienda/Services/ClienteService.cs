using System.Net.Http.Json;
using System.Text.Json;
using Tienda.Models;

namespace Tienda.Services;

public class ClienteService : IClienteService
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ClienteService(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<List<Address>> GetDireccionesAsync(int clienteId)
    {
        try
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"/api/clientes/{clienteId}/direcciones");
            if (!response.IsSuccessStatusCode) return [];

            var json  = await response.Content.ReadAsStringAsync();
            var dtos  = JsonSerializer.Deserialize<List<DireccionDto>>(json, JsonOpts) ?? [];
            return dtos.Select(MapToAddress).ToList();
        }
        catch { return []; }
    }

    public async Task<Address?> AgregarDireccionAsync(int clienteId, Address address)
    {
        try
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync(
                $"/api/clientes/{clienteId}/direcciones",
                new
                {
                    nombre       = address.Name,
                    calle        = address.Street,
                    ciudad       = address.City,
                    estado       = address.State,
                    codigoPostal = address.ZipCode,
                    pais         = address.Country,
                    esPrincipal  = address.IsDefault
                });

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var dto  = JsonSerializer.Deserialize<DireccionDto>(json, JsonOpts);
            return dto is null ? null : MapToAddress(dto);
        }
        catch { return null; }
    }

    public async Task<bool> EliminarDireccionAsync(int clienteId, int direccionId)
    {
        try
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync(
                $"/api/clientes/{clienteId}/direcciones/{direccionId}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    private void SetAuthHeader()
    {
        var token = _auth.GetToken();
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private static Address MapToAddress(DireccionDto dto) => new()
    {
        Id        = dto.Id,
        Name      = dto.Nombre,
        Street    = dto.Calle,
        City      = dto.Ciudad,
        State     = dto.Estado,
        ZipCode   = dto.CodigoPostal,
        Country   = dto.Pais,
        IsDefault = dto.EsPrincipal
    };
}
