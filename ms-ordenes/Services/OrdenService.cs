using Microsoft.EntityFrameworkCore;
using MsOrdenes.Data;
using MsOrdenes.Events;
using MsOrdenes.Models;

namespace MsOrdenes.Services;

public class OrdenService
{
    private readonly OrdenesDbContext _db;
    private readonly RabbitMQPublisher _publisher;

    public OrdenService(OrdenesDbContext db, RabbitMQPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<OrdenResponse> CrearOrdenAsync(CrearOrdenRequest request)
    {
        // 1. Calcular subtotal
        decimal subtotal = request.Items.Sum(i => i.PrecioUnitario * i.Cantidad);

        // 2. Calcular descuento por categoría: electrónicos 5%, otros >= $1000 → 10%
        decimal subtotalElectronicos = request.Items
            .Where(i => i.EsElectronico)
            .Sum(i => i.PrecioUnitario * i.Cantidad);
        decimal subtotalOtros = subtotal - subtotalElectronicos;

        decimal descuentoElectronicos = subtotalElectronicos > 0 ? subtotalElectronicos * 0.05m : 0;
        decimal descuentoOtros        = subtotalOtros >= 1000    ? subtotalOtros        * 0.10m : 0;
        decimal descuentoMonto        = descuentoElectronicos + descuentoOtros;
        decimal descuentoPct          = subtotal > 0 ? Math.Round(descuentoMonto / subtotal * 100, 2) : 0;
        decimal total = subtotal - descuentoMonto;

        // 3. Calcular meses MSI
        int mesesMsi = (request.ModalidadPago ?? "").ToLower().Replace(" ", "") switch
        {
            "3msi" => 3,
            "6msi" => 6,
            "9msi" => 9,
            _ => 0
        };

        // 4. Crear la orden
        var orden = new Orden
        {
            UsuarioId = request.UsuarioId,
            Estado = "pendiente",
            DireccionEntrega = request.DireccionEntrega,
            Subtotal = subtotal,
            DescuentoPct = descuentoPct,
            DescuentoMonto = descuentoMonto,
            Total = total,
            ModalidadPago = request.ModalidadPago,
            MesesMsi = mesesMsi,
            CreadoEn = DateTime.UtcNow,
            ActualizadoEn = DateTime.UtcNow,
            Items = request.Items.Select(i => new ItemOrden
            {
                ProductoId = i.ProductoId,
                NombreProducto = i.NombreProducto,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Subtotal = i.PrecioUnitario * i.Cantidad,
                EsElectronico = i.EsElectronico
            }).ToList()
        };

        _db.Ordenes.Add(orden);
        await _db.SaveChangesAsync();

        // 5. Publicar evento OrdenCreada en RabbitMQ
        var evento = new OrdenCreadaEvent
        {
            OrdenId = orden.Id,
            ClienteId = orden.UsuarioId,
            Total = orden.Total,
            Descuento = orden.DescuentoMonto,
            ModalidadPago = orden.ModalidadPago,
            MesesMsi = orden.MesesMsi,
            DireccionEntrega = orden.DireccionEntrega,
            CreadoEn = DateTime.UtcNow,
            Items = orden.Items.Select(i => new ItemEvent
            {
                ProductoId = i.ProductoId,
                NombreProducto = i.NombreProducto,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                EsElectronico = i.EsElectronico
            }).ToList()
        };

        await _publisher.PublicarAsync("tienda.ordenes", "orden.creada", evento);

        // 6. Retornar respuesta
        return new OrdenResponse
        {
            Id = orden.Id,
            UsuarioId = orden.UsuarioId,
            Estado = orden.Estado,
            Subtotal = orden.Subtotal,
            DescuentoPct = orden.DescuentoPct,
            DescuentoMonto = orden.DescuentoMonto,
            Total = orden.Total,
            ModalidadPago = orden.ModalidadPago,
            MesesMsi = orden.MesesMsi,
            CreadoEn = orden.CreadoEn,
            Items = orden.Items.Select(i => new ItemOrdenResponse
            {
                ProductoId = i.ProductoId,
                NombreProducto = i.NombreProducto,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Subtotal = i.Subtotal,
                EsElectronico = i.EsElectronico
            }).ToList()
        };
    }

    public async Task<List<OrdenResponse>> ObtenerOrdenesPorUsuarioAsync(int usuarioId)
    {
        var ordenes = await _db.Ordenes
            .Include(o => o.Items)
            .Where(o => o.UsuarioId == usuarioId)
            .ToListAsync();

        return ordenes.Select(orden => new OrdenResponse
        {
            Id = orden.Id,
            UsuarioId = orden.UsuarioId,
            Estado = orden.Estado,
            Subtotal = orden.Subtotal,
            DescuentoPct = orden.DescuentoPct,
            DescuentoMonto = orden.DescuentoMonto,
            Total = orden.Total,
            ModalidadPago = orden.ModalidadPago,
            MesesMsi = orden.MesesMsi,
            CreadoEn = orden.CreadoEn,
            Items = orden.Items.Select(i => new ItemOrdenResponse
            {
                ProductoId = i.ProductoId,
                NombreProducto = i.NombreProducto,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Subtotal = i.Subtotal,
                EsElectronico = i.EsElectronico
            }).ToList()
        }).ToList();
    }

    public async Task<OrdenResponse?> ObtenerOrdenPorIdAsync(int id)
    {
        var orden = await _db.Ordenes
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden is null) return null;

        return new OrdenResponse
        {
            Id = orden.Id,
            UsuarioId = orden.UsuarioId,
            Estado = orden.Estado,
            Subtotal = orden.Subtotal,
            DescuentoPct = orden.DescuentoPct,
            DescuentoMonto = orden.DescuentoMonto,
            Total = orden.Total,
            ModalidadPago = orden.ModalidadPago,
            MesesMsi = orden.MesesMsi,
            CreadoEn = orden.CreadoEn,
            Items = orden.Items.Select(i => new ItemOrdenResponse
            {
                ProductoId = i.ProductoId,
                NombreProducto = i.NombreProducto,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Subtotal = i.Subtotal,
                EsElectronico = i.EsElectronico
            }).ToList()
        };
    }
}