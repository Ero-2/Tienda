namespace MsPagos.Models;

public class PlanPago
{
    public int      Id             { get; set; }
    public int      OrdenId        { get; set; }
    public int      UsuarioId      { get; set; }
    public decimal  TotalFinanciado { get; set; }
    public int      MesesMsi       { get; set; }
    public decimal  CuotaMensual   { get; set; }
    public int      CuotasPagadas  { get; set; } = 1;
    public string   Estado         { get; set; } = "activo";   // activo | completado
    public DateTime CreadoEn       { get; set; } = DateTime.UtcNow;
}

public class PlanPagoResponse
{
    public int      Id              { get; set; }
    public int      OrdenId         { get; set; }
    public decimal  TotalFinanciado  { get; set; }
    public int      MesesMsi        { get; set; }
    public decimal  CuotaMensual    { get; set; }
    public int      CuotasPagadas   { get; set; }
    public int      CuotasPendientes => MesesMsi - CuotasPagadas;
    public decimal  SaldoPendiente  { get; set; }
    public string   Estado          { get; set; } = string.Empty;
    public DateTime CreadoEn        { get; set; }
}
