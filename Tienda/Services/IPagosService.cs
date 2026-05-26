using Tienda.Models;

namespace Tienda.Services;

public interface IPagosService
{
    Task<CheckoutInfo?> ObtenerCheckoutAsync(int ordenId);
    Task<ConfirmacionPago?> ConfirmarPagoAsync(string token, TarjetaData tarjeta);
    Task<List<PlanPagoInfo>> ObtenerPlanesAsync(int usuarioId);
    Task<(bool ok, string? error)> PagarCuotaAsync(int planId, int usuarioId, TarjetaData tarjeta);
}

public class PlanPagoInfo
{
    public int      Id               { get; set; }
    public int      OrdenId          { get; set; }
    public decimal  TotalFinanciado   { get; set; }
    public int      MesesMsi         { get; set; }
    public decimal  CuotaMensual     { get; set; }
    public int      CuotasPagadas    { get; set; }
    public int      CuotasPendientes { get; set; }
    public decimal  SaldoPendiente   { get; set; }
    public string   Estado           { get; set; } = string.Empty;
    public DateTime CreadoEn         { get; set; }
}

public class CheckoutInfo
{
    public string Token   { get; set; } = string.Empty;
    public int    OrdenId { get; set; }
    public decimal Monto  { get; set; }
    public string Estado  { get; set; } = string.Empty;
}

public class ConfirmacionPago
{
    public string  Estado              { get; set; } = string.Empty;
    public string  Mensaje             { get; set; } = string.Empty;
    public string? TransaccionId       { get; set; }
    public string? TarjetaEnmascarada  { get; set; }
    public string? MarcaTarjeta        { get; set; }
    public bool    YaProcesado         { get; set; }
}

public class TarjetaData
{
    public string NumeroTarjeta  { get; set; } = string.Empty;
    public string NombreTitular  { get; set; } = string.Empty;
    public int    Mes            { get; set; }
    public int    Anio           { get; set; }
    public string Cvv            { get; set; } = string.Empty;
}
