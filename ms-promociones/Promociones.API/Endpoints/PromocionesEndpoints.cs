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

            // POST: Calcular Descuento (¡Aquí aplicamos las reglas de negocio del profe!)
            group.MapPost("/calcular", (SolicitudDescuento req) =>
            {
                decimal descuento = 0;
                string motivo = "Sin descuento";

                // Regla 1: Exclusivamente electrónicos -> 5% sin importar el monto
                if (req.EsSoloElectronica)
                {
                    descuento = req.Total * 0.05m;
                    motivo = "Descuento del 5% por compra de solo electrónicos";
                }
                // Regla 2: Compra superior a $10,000 MXN -> 10%
                else if (req.Total >= 10000)
                {
                    descuento = req.Total * 0.10m;
                    motivo = "Descuento del 10% por compra superior a $10,000 MXN";
                }

                // Manejo de responses: 200 OK con el desglose exacto
                return Results.Ok(new
                {
                    TotalOriginal = req.Total,
                    DescuentoAplicado = descuento,
                    TotalAPagar = req.Total - descuento,
                    Motivo = motivo
                });
            });
        }
    }

    // Modelo temporal para recibir los datos del carrito
    public class SolicitudDescuento
    {
        public decimal Total { get; set; }
        public bool EsSoloElectronica { get; set; }
    }
}