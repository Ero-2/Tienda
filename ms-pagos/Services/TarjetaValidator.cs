using MsPagos.Models;

namespace MsPagos.Services;

public static class TarjetaValidator
{
    public static List<string> Validar(TarjetaRequest t)
    {
        var errores = new List<string>();
        var numero = Normalizar(t.NumeroTarjeta);

        if (string.IsNullOrWhiteSpace(numero))
            errores.Add("El número de tarjeta es obligatorio.");
        else if (!numero.All(char.IsDigit))
            errores.Add("El número de tarjeta solo debe contener dígitos.");
        else if (numero.Length < 13 || numero.Length > 19)
            errores.Add("El número de tarjeta debe tener entre 13 y 19 dígitos.");
        else if (!PasaLuhn(numero))
            errores.Add("El número de tarjeta no es válido (no pasa la verificación de Luhn).");

        if (string.IsNullOrWhiteSpace(t.NombreTitular))
            errores.Add("El nombre del titular es obligatorio.");

        if (t.Mes < 1 || t.Mes > 12)
            errores.Add("El mes de expiración debe estar entre 1 y 12.");

        if (t.Anio < 2000 || t.Anio > 2100)
            errores.Add("El año de expiración no es válido.");

        if (t.Mes is >= 1 and <= 12 && t.Anio is >= 2000 and <= 2100)
        {
            var ultimoDiaDelMes = new DateTime(t.Anio, t.Mes, DateTime.DaysInMonth(t.Anio, t.Mes));
            if (ultimoDiaDelMes < DateTime.UtcNow.Date)
                errores.Add("La tarjeta está expirada.");
        }

        if (string.IsNullOrWhiteSpace(t.Cvv) || !t.Cvv.All(char.IsDigit) || t.Cvv.Length is < 3 or > 4)
            errores.Add("El CVV debe tener 3 o 4 dígitos.");

        return errores;
    }

    public static bool PasaLuhn(string numero)
    {
        var suma = 0;
        var alternar = false;
        for (var i = numero.Length - 1; i >= 0; i--)
        {
            var digito = numero[i] - '0';
            if (alternar) { digito *= 2; if (digito > 9) digito -= 9; }
            suma += digito;
            alternar = !alternar;
        }
        return suma % 10 == 0;
    }

    public static string Enmascarar(string numeroTarjeta)
    {
        var numero = Normalizar(numeroTarjeta);
        return numero.Length < 4 ? "****" : $"**** **** **** {numero[^4..]}";
    }

    public static string DetectarMarca(string numeroTarjeta)
    {
        var n = Normalizar(numeroTarjeta);
        if (n.Length == 0) return "Desconocida";
        if (n.StartsWith("4")) return "Visa";
        if (n.Length >= 4 && n.StartsWith("6011")) return "Discover";
        if (n.Length >= 2)
        {
            var pref2 = int.Parse(n[..2]);
            if (pref2 is >= 51 and <= 55) return "MasterCard";
            if (pref2 is 34 or 37) return "American Express";
        }
        return "Desconocida";
    }

    private static string Normalizar(string? numero) =>
        (numero ?? string.Empty).Replace(" ", "").Replace("-", "");
}
