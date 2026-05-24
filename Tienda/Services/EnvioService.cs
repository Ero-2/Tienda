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
}
