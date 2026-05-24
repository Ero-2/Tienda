using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MsPagos.Services;

public class OpenPayService
{
    private readonly HttpClient _http;
    private readonly string _merchantId;
    private readonly ILogger<OpenPayService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenPayService(IConfiguration config, ILogger<OpenPayService> logger)
    {
        _logger = logger;
        _merchantId = config["OpenPay:MerchantId"]!;
        var privateKey = config["OpenPay:PrivateKey"]!;
        var baseUrl    = config["OpenPay:BaseUrl"]!;

        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{privateKey}:"));
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<OpenPayResult> ProcesarPagoAsync(
        decimal monto, int mesesMsi, string cardToken, int ordenId)
    {
        try
        {
            object body;

            if (mesesMsi > 0)
            {
                body = new
                {
                    method           = "card",
                    source_id        = cardToken,
                    amount           = monto,
                    currency         = "MXN",
                    description      = $"Orden #{ordenId}",
                    device_session_id = Guid.NewGuid().ToString("N"),
                    payment_plan     = new { payments = mesesMsi }
                };
            }
            else
            {
                body = new
                {
                    method            = "card",
                    source_id         = cardToken,
                    amount            = monto,
                    currency          = "MXN",
                    description       = $"Orden #{ordenId}",
                    device_session_id = Guid.NewGuid().ToString("N")
                };
            }

            var json    = JsonSerializer.Serialize(body, JsonOpts);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(
                $"{_merchantId}/charges", content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("OpenPay response {Status}: {Body}",
                (int)response.StatusCode, responseBody);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                return new OpenPayResult
                {
                    Exitoso       = true,
                    TransaccionId = root.GetProperty("id").GetString(),
                    Referencia    = root.TryGetProperty("authorization", out var auth)
                                    ? auth.GetString() : null,
                    Mensaje       = "Pago aprobado"
                };
            }
            else
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                var errorCode = root.TryGetProperty("error_code", out var ec)
                                ? ec.GetInt32().ToString() : "9999";
                var desc = root.TryGetProperty("description", out var d)
                           ? d.GetString() : "Error al procesar pago";

                return new OpenPayResult
                {
                    Exitoso      = false,
                    ErrorCodigo  = errorCode,
                    Mensaje      = desc ?? "Error al procesar pago"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error llamando OpenPay");
            return new OpenPayResult
            {
                Exitoso     = false,
                ErrorCodigo = "9999",
                Mensaje     = "Error interno al procesar pago"
            };
        }
    }
}

public class OpenPayResult
{
    public bool    Exitoso       { get; set; }
    public string? TransaccionId { get; set; }
    public string? Referencia    { get; set; }
    public string? ErrorCodigo   { get; set; }
    public string  Mensaje       { get; set; } = string.Empty;
}
