// Tienda/Services/OrdenService.cs
using System.Net.Http.Json;
using System.Text.Json;
using Tienda.Models;

namespace Tienda.Services;

public class OrdenService : IOrdenService
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OrdenService(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<OrdenResponse?> CrearOrdenAsync(List<CartItem> items, string modalidadPago, string direccionEntrega = "")
    {
        SetAuthHeader();

        var request = new
        {
            usuarioId        = _auth.GetUserId(),
            modalidadPago    = MapModalidad(modalidadPago),
            direccionEntrega = direccionEntrega,
            items            = items.Select(i => new
            {
                productoId      = i.Product.Id,
                nombreProducto  = i.Product.Name,
                precioUnitario  = i.Product.Price,
                cantidad        = i.Quantity,
                esElectronico   = i.Product.Category?.ToLower().Contains("electr") == true
            })
        };

        var idempotencyKey = Guid.NewGuid().ToString();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/ordenes");
        req.Headers.Add("Idempotency-Key", idempotencyKey);
        req.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode}: {body}", null, response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrdenResponse>(json, JsonOpts);
    }

    public async Task<List<OrdenResponse>> ObtenerMisOrdenesAsync()
    {
        SetAuthHeader();

        var userId = _auth.GetUserId();
        var response = await _http.GetAsync($"/api/ordenes/usuario/{userId}");

        if (!response.IsSuccessStatusCode) return new List<OrdenResponse>();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<OrdenResponse>>(json, JsonOpts)
               ?? new List<OrdenResponse>();
    }

    public async Task<OrdenResponse?> ObtenerOrdenPorIdAsync(int id)
    {
        SetAuthHeader();

        var response = await _http.GetAsync($"/api/ordenes/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrdenResponse>(json, JsonOpts);
    }

    private void SetAuthHeader()
    {
        var token = _auth.GetToken();
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private static string MapModalidad(string uiValue) => uiValue switch
    {
        "3 MSI"  => "3msi",
        "6 MSI"  => "6msi",
        "12 MSI" => "12msi",
        _        => "contado"
    };
}
