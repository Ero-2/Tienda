using Microsoft.EntityFrameworkCore;
using Promociones.API.Data;
using Promociones.API.Models;
using System;

namespace Promociones.API.Endpoints
{
    public static class PromocionesEndpoints
    {
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/api/promociones");

            // GET: Ver promociones activas
            group.MapGet("/", async (AppDbContext db) =>
            {
                return Results.Ok(await db.Promociones.Where(p => p.Activa).ToListAsync());
            });

            // POST: Crear una promoción nueva
            group.MapPost("/", async (Promocion nuevaPromo, AppDbContext db) =>
            {
                db.Promociones.Add(nuevaPromo);
                await db.SaveChangesAsync();
                return Results.Created($"/api/promociones/{nuevaPromo.Id}", nuevaPromo);
            });

            // POST: Calcular Descuento
            group.MapPost("/calcular", (SolicitudDescuento req) =>
            {
                // Electrónicos: 5% sobre su subtotal
                decimal descuentoElectronicos = req.SubtotalElectronicos * 0.05m;
                // Otros: 10% si subtotal >= $1,000
                decimal descuentoOtros = req.SubtotalOtros >= 1000 ? req.SubtotalOtros * 0.10m : 0;

                decimal total     = req.SubtotalElectronicos + req.SubtotalOtros;
                decimal descuento = descuentoElectronicos + descuentoOtros;

                string motivo = (descuentoElectronicos > 0, descuentoOtros > 0) switch
                {
                    (true,  true)  => $"5% electrónicos + 10% otros (≥$1,000)",
                    (true,  false) => "Descuento del 5% en electrónicos",
                    (false, true)  => "Descuento del 10% por compra superior a $1,000",
                    _              => "Sin descuento"
                };

                return Results.Ok(new
                {
                    TotalOriginal     = total,
                    DescuentoAplicado = descuento,
                    TotalAPagar       = total - descuento,
                    Motivo            = motivo
                });
            });
        }
    }

    public class SolicitudDescuento
    {
        public decimal SubtotalElectronicos { get; set; }
        public decimal SubtotalOtros        { get; set; }
    }
}