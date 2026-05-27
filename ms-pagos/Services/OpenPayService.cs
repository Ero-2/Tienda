using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MsPagos.Models;

namespace MsPagos.Services;

public class OpenPayService
{
    private readonly HttpClient _httpPublic;
    private readonly HttpClient _httpPrivate;
    private readonly ILogger<OpenPayService> _logger;

    public OpenPayService(IConfiguration config, ILogger<OpenPayService> logger)
    {
        _logger = logger;

        var merchantId = config["OpenPay:MerchantId"]
            ?? throw new InvalidOperationException("Falta OpenPay:MerchantId en configuración.");
        var publicKey  = config["OpenPay:PublicKey"]
            ?? throw new InvalidOperationException("Falta OpenPay:PublicKey en configuración.");
        var privateKey = config["OpenPay:PrivateKey"]
            ?? throw new InvalidOperationException("Falta OpenPay:PrivateKey en configuración.");

        var baseUrl = $"https://sandbox-api.openpay.mx/v1/{merchantId}/";

        _httpPublic  = CrearCliente(baseUrl, publicKey);
        _httpPrivate = CrearCliente(baseUrl, privateKey);
    }

    public async Task<OpenPayResult> ProcesarPagoConTarjetaAsync(
        decimal monto, int mesesMsi, TarjetaRequest tarjeta)
    {
        var tokenId = await TokenizarAsync(tarjeta);
        if (tokenId is null)
            return new OpenPayResult
            {
                Estado      = "rechazado",
                CodigoError = "tokenization_error",
                Mensaje     = "No se pudo tokenizar la tarjeta con OpenPay."
            };

        return await CobrarAsync(monto, mesesMsi, tokenId, tarjeta);
    }

    // ── Paso 1: tokenizar tarjeta (public key) ───────────────────────────────
    private async Task<string?> TokenizarAsync(TarjetaRequest tarjeta)
    {
        var body = new Dictionary<string, object>
        {
            ["card_number"]      = Limpiar(tarjeta.NumeroTarjeta),
            ["holder_name"]      = tarjeta.NombreTitular,
            ["expiration_year"]  = (tarjeta.Anio % 100).ToString("D2"),
            ["expiration_month"] = tarjeta.Mes.ToString("D2"),
            ["cvv2"]             = tarjeta.Cvv
        };

        try
        {
            var json     = JsonSerializer.Serialize(body);
            var response = await _httpPublic.PostAsync("tokens",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("OpenPay /tokens [{Status}]: {Body}",
                (int)response.StatusCode, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Tokenización fallida: {Body}", content);
                return null;
            }

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("id").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción en tokenización OpenPay");
            return null;
        }
    }

    // ── Paso 2: cobrar con token (private key) ───────────────────────────────
    private async Task<OpenPayResult> CobrarAsync(decimal monto, int mesesMsi, string tokenId, TarjetaRequest tarjeta)
    {
        var partes    = tarjeta.NombreTitular.Trim().Split(' ', 2);
        var nombre    = partes[0];
        var apellido  = partes.Length > 1 ? partes[1] : nombre;
        var email     = string.IsNullOrWhiteSpace(tarjeta.Email)
                        ? "cliente@tienda.mx"
                        : tarjeta.Email;

        var body = new Dictionary<string, object>
        {
            ["method"]            = "card",
            ["source_id"]         = tokenId,
            ["amount"]            = Math.Round(monto, 2),
            ["currency"]          = "MXN",
            ["description"]       = "Pago tienda departamental",
            ["device_session_id"] = Guid.NewGuid().ToString("N"),
            ["customer"]          = new Dictionary<string, string>
            {
                ["name"]       = nombre,
                ["last_name"]  = apellido,
                ["email"]      = email
            }
        };

        if (mesesMsi > 1)
            body["payment_plan"] = new { payments = mesesMsi };

        try
        {
            var json     = JsonSerializer.Serialize(body);
            var response = await _httpPrivate.PostAsync("charges",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("OpenPay /charges [{Status}]: {Body}",
                (int)response.StatusCode, content);

            using var doc = JsonDocument.Parse(content);
            var root      = doc.RootElement;

            if (response.IsSuccessStatusCode)
            {
                var id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                return new OpenPayResult
                {
                    Estado        = "aprobado",
                    TransaccionId = id,
                    Referencia    = id,
                    Mensaje       = "Pago aprobado por OpenPay."
                };
            }

            var errCode = root.TryGetProperty("error_code", out var ecProp)
                          ? ecProp.GetRawText() : "9999";
            var errDesc = root.TryGetProperty("description", out var descProp)
                          ? descProp.GetString() : "Error desconocido";

            return new OpenPayResult
            {
                Estado      = "rechazado",
                CodigoError = errCode,
                Mensaje     = errDesc ?? "Error al procesar el pago."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción en cobro OpenPay");
            return new OpenPayResult
            {
                Estado      = "rechazado",
                CodigoError = "gateway_error",
                Mensaje     = $"Error al contactar OpenPay: {ex.Message}"
            };
        }
    }

    private static HttpClient CrearCliente(string baseUrl, string apiKey)
    {
        var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        var token  = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", token);
        return client;
    }

    private static string Limpiar(string s) =>
        (s ?? string.Empty).Replace(" ", "").Replace("-", "");
}

public class OpenPayResult
{
    public string  Estado        { get; set; } = string.Empty;
    public string? TransaccionId { get; set; }
    public string? Referencia    { get; set; }
    public string? CodigoError   { get; set; }
    public string  Mensaje       { get; set; } = string.Empty;
}

public class OpenPayTransitorioException : Exception
{
    public OpenPayTransitorioException(string message) : base(message) { }
}
