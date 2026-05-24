using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Tienda.Services;

public interface IOpenPayTokenService
{
    Task<string?> TokenizarTarjetaAsync(
        string numero, string titular, string mesExp, string anioExp, string cvv);
}

public class OpenPayTokenService : IOpenPayTokenService
{
    private const string MerchantId = "mcygu87wscna6wr5bwes";
    private const string PublicKey  = "pk_df4e4734ad1245358618d303e0572ea5";
    private const string BaseUrl    = "https://sandbox-api.openpay.mx/v1/";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string?> TokenizarTarjetaAsync(
        string numero, string titular, string mesExp, string anioExp, string cvv)
    {
        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{PublicKey}:"));
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        var body = JsonSerializer.Serialize(new
        {
            card_number      = numero.Replace(" ", ""),
            holder_name      = titular,
            expiration_month = mesExp,
            expiration_year  = anioExp,
            cvv2             = cvv
        });

        var response = await http.PostAsync(
            $"{MerchantId}/tokens",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString();
    }
}
