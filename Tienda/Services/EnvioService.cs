using System.Net.Http.Headers;
using System.Text.Json;
using Tienda.Models;

namespace Tienda.Services;

public class EnvioService : IEnvioService
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public EnvioService(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<EnvioRastreoResponse?> GetRastreoAsync(int orderId)
    {
        try
        {
            var token = _auth.GetToken();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.GetAsync($"/api/envios/rastreo/{orderId}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EnvioRastreoResponse>(json, JsonOpts);
        }
        catch { return null; }
    }

    public async Task<CostoEnvioDto> CalcularCostoAsync(decimal subtotal)
    {
        try
        {
            var response = await _http.GetAsync($"/api/envios/calcular-costo?subtotal={subtotal}");
            if (!response.IsSuccessStatusCode)
                return FallbackCosto(subtotal);

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CostoEnvioDto>(json, JsonOpts) ?? FallbackCosto(subtotal);
        }
        catch { return FallbackCosto(subtotal); }
    }

    private static CostoEnvioDto FallbackCosto(decimal subtotal)
    {
        bool gratis = subtotal >= 50m;
        return new CostoEnvioDto
        {
            CostoEnvio       = gratis ? 0m : 9.99m,
            EsGratis         = gratis,
            MensajePromocion = gratis ? "Envío gratis" : "Envío $9.99"
        };
    }
}
