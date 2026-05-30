// Tienda/Services/AuthService.cs
using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tienda.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;

    private const string IsLoggedInKey = "isLoggedIn";
    private const string UserEmailKey  = "userEmail";
    private const string TokenKey      = "jwtToken";
    private const string UserIdKey     = "userId";
    private const string UserNameKey   = "userName";

    private const string IsGuestKey = "isGuest";

    public bool IsGuest => Preferences.Get(IsGuestKey, false);

    public void ContinueAsGuest()
        => Preferences.Set(IsGuestKey, true);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Sesión válida = bandera activa + token presente y no expirado.
    // Si el token caducó (o falta), se limpia la sesión para no mostrar un perfil fantasma.
    public bool IsLoggedIn
    {
        get
        {
            if (!Preferences.Get(IsLoggedInKey, false)) return false;

            var token = Preferences.Get(TokenKey, string.Empty);
            if (string.IsNullOrWhiteSpace(token) || TokenExpirado(token))
            {
                Logout();
                return false;
            }
            return true;
        }
    }

    public AuthService(HttpClient http) => _http = http;

    // ── Login / Registro reales ───────────────────────────────

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "/api/clientes/login",
                new { email, password });

            if (!response.IsSuccessStatusCode)
                return (false, "Email o contraseña incorrectos");

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponseDto>(json, JsonOpts);
            if (result is null) return (false, "Error al procesar respuesta");

            GuardarSesion(result);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(
        string nombre, string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "/api/clientes/registro",
                new { nombre, email, password });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var msg = doc.RootElement.GetProperty("error").GetString();
                    return (false, msg ?? "Error al registrarse");
                }
                catch { return (false, "Error al registrarse"); }
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponseDto>(json, JsonOpts);
            if (result is null) return (false, "Error al procesar respuesta");

            GuardarSesion(result);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.GetType().Name} - {ex.Message}");
        }
    }

    // ── Sesión ────────────────────────────────────────────────

    public void Logout()
    {
        Preferences.Remove(IsLoggedInKey);
        Preferences.Remove(UserEmailKey);
        Preferences.Remove(TokenKey);
        Preferences.Remove(UserIdKey);
        Preferences.Remove(UserNameKey);
        Preferences.Remove(IsGuestKey);
    }

    public string? GetUserEmail() => Preferences.Get(UserEmailKey, string.Empty);
    public string? GetToken()     => Preferences.Get(TokenKey,     string.Empty);
    public int     GetUserId()    => Preferences.Get(UserIdKey,    0);

    // ── Helpers ───────────────────────────────────────────────

    private static void GuardarSesion(LoginResponseDto result)
    {
        Preferences.Set(IsLoggedInKey, true);
        Preferences.Set(UserEmailKey,  result.Email);
        Preferences.Set(TokenKey,      result.Token);
        Preferences.Set(UserIdKey,     result.ClienteId);
        Preferences.Set(UserNameKey,   result.Nombre);
    }

    // Lee el claim "exp" del JWT (sin validar firma) y determina si ya caducó.
    private static bool TokenExpirado(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return true;

            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("exp", out var expEl)) return true;

            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expEl.GetInt64();
        }
        catch { return true; }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        s += (s.Length % 4) switch { 2 => "==", 3 => "=", _ => string.Empty };
        return Convert.FromBase64String(s);
    }
}
