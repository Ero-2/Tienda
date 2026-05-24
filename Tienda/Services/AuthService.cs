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

    // Usada solo para logins de invitado
    private const string JwtSecret   = "T13nd4D3p4rtam3nt4l-S3cr3tK3y-2024!@#$";
    private const string JwtIssuer   = "ApiGateway.TiendaDepartamental";
    private const string JwtAudience = "TiendaDepartamental.Clientes";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool IsLoggedIn => Preferences.Get(IsLoggedInKey, false);

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

            var json   = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponseDto>(json, JsonOpts);
            if (result is null) return (false, "Error al procesar respuesta");

            GuardarSesion(result);
            return (true, null);
        }
        catch
        {
            return (false, "No se pudo conectar al servidor");
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

            var json   = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponseDto>(json, JsonOpts);
            if (result is null) return (false, "Error al procesar respuesta");

            GuardarSesion(result);
            return (true, null);
        }
        catch
        {
            return (false, "No se pudo conectar al servidor");
        }
    }

    // ── Invitado ──────────────────────────────────────────────

    public void LoginAsGuest(string email)
    {
        var token = GenerateGuestJwt(email);
        Preferences.Set(IsLoggedInKey, true);
        Preferences.Set(UserEmailKey,  email);
        Preferences.Set(TokenKey,      token);
        Preferences.Set(UserIdKey,     0);
        Preferences.Set(UserNameKey,   email.Split('@')[0]);
    }

    // ── Sesión ────────────────────────────────────────────────

    public void Logout()
    {
        Preferences.Remove(IsLoggedInKey);
        Preferences.Remove(UserEmailKey);
        Preferences.Remove(TokenKey);
        Preferences.Remove(UserIdKey);
        Preferences.Remove(UserNameKey);
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

    private static string GenerateGuestJwt(string email)
    {
        var header  = Base64UrlEncode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
        var exp     = DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeSeconds();
        var payload = Base64UrlEncode(
            $"{{\"sub\":\"0\",\"email\":\"{email}\"," +
            $"\"iss\":\"{JwtIssuer}\",\"aud\":\"{JwtAudience}\",\"exp\":{exp}}}");

        var signing = $"{header}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(JwtSecret));
        var sig = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(signing)));

        return $"{header}.{payload}.{sig}";
    }

    private static string Base64UrlEncode(string text) =>
        Base64UrlEncode(Encoding.UTF8.GetBytes(text));

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
