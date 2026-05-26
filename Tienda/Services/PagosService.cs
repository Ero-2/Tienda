using System.Net.Http.Json;
using System.Text.Json;
using Tienda.Models;

namespace Tienda.Services;

public class PagosService : IPagosService
{
    private readonly HttpClient   _http;
    private readonly IAuthService _auth;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PagosService(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<CheckoutInfo?> ObtenerCheckoutAsync(int ordenId)
    {
        SetAuthHeader();

        // Obtiene el pago (con tokenCheckout) de la orden recién creada
        var response = await _http.GetAsync($"/api/pagos/orden/{ordenId}");
        if (!response.IsSuccessStatusCode) return null;

        var json  = await response.Content.ReadAsStringAsync();
        var pagos = JsonSerializer.Deserialize<List<CheckoutInfo>>(json, JsonOpts);
        return pagos?.FirstOrDefault();
    }

    public async Task<ConfirmacionPago?> ConfirmarPagoAsync(string token, TarjetaData tarjeta)
    {
        SetAuthHeader();

        var body = new
        {
            numeroTarjeta = tarjeta.NumeroTarjeta,
            nombreTitular = tarjeta.NombreTitular,
            mes           = tarjeta.Mes,
            anio          = tarjeta.Anio,
            cvv           = tarjeta.Cvv
        };

        var response = await _http.PostAsJsonAsync($"/api/pagos/checkout/{token}/confirmar", body);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {err}", null, response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfirmacionPago>(json, JsonOpts);
    }

    public async Task<List<PlanPagoInfo>> ObtenerPlanesAsync(int usuarioId)
    {
        try
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"/api/pagos/planes/{usuarioId}");
            if (!response.IsSuccessStatusCode) return [];
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PlanPagoInfo>>(json, JsonOpts) ?? [];
        }
        catch { return []; }
    }

    public async Task<(bool ok, string? error)> PagarCuotaAsync(int planId, int usuarioId, TarjetaData tarjeta)
    {
        try
        {
            SetAuthHeader();
            var body = new
            {
                usuarioId,
                tarjeta = new
                {
                    numeroTarjeta = tarjeta.NumeroTarjeta,
                    nombreTitular = tarjeta.NombreTitular,
                    mes           = tarjeta.Mes,
                    anio          = tarjeta.Anio,
                    cvv           = tarjeta.Cvv
                }
            };
            var response = await _http.PostAsJsonAsync($"/api/pagos/planes/{planId}/pagar", body);
            if (response.IsSuccessStatusCode) return (true, null);

            var err = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(err);
                return (false, doc.RootElement.GetProperty("error").GetString());
            }
            catch { return (false, "Error al pagar cuota"); }
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    private void SetAuthHeader()
    {
        var token = _auth.GetToken();
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
