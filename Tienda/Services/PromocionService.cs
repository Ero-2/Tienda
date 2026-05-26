using System.Net.Http.Json;
using System.Text.Json;
using Tienda.Models;

namespace Tienda.Services;

public class PromocionService : IPromocionService
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public PromocionService(HttpClient http) => _http = http;

    public async Task<List<Promocion>> GetPromocionesActivasAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/promociones");
            if (!response.IsSuccessStatusCode) return [];
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Promocion>>(json, JsonOpts) ?? [];
        }
        catch { return []; }
    }

    public async Task<DescuentoResponse?> CalcularDescuentoAsync(decimal subtotalElectronicos, decimal subtotalOtros)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/promociones/calcular",
                new { SubtotalElectronicos = subtotalElectronicos, SubtotalOtros = subtotalOtros });
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DescuentoResponse>(json, JsonOpts);
        }
        catch { return null; }
    }
}
