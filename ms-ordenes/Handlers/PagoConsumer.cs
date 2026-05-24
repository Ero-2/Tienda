using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using MsOrdenes.Events;
using MsOrdenes.Data;
using Microsoft.EntityFrameworkCore;

namespace MsOrdenes.Handlers;

public class PagoConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PagoConsumer> _logger;

    public PagoConsumer(IConfiguration config, IServiceScopeFactory scopeFactory, ILogger<PagoConsumer> logger)
    {
        _config = config;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["RabbitMQ:Host"] ?? "rabbitmq",
                    Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                    UserName = _config["RabbitMQ:User"] ?? "admin",
                    Password = _config["RabbitMQ:Password"] ?? "Admin_12345"
                };

                using var connection = await factory.CreateConnectionAsync(stoppingToken);
                using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                // Declarar exchange de pagos
                await channel.ExchangeDeclareAsync(
                    exchange: "tienda.pagos",
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: stoppingToken
                );

                // Cola para PagoAprobado
                await channel.QueueDeclareAsync(
                    queue: "ordenes.pago-aprobado",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken
                );

                await channel.QueueBindAsync(
                    queue: "ordenes.pago-aprobado",
                    exchange: "tienda.pagos",
                    routingKey: "pago.aprobado",
                    cancellationToken: stoppingToken
                );

                // Cola para PagoRechazado
                await channel.QueueDeclareAsync(
                    queue: "ordenes.pago-rechazado",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken
                );

                await channel.QueueBindAsync(
                    queue: "ordenes.pago-rechazado",
                    exchange: "tienda.pagos",
                    routingKey: "pago.rechazado",
                    cancellationToken: stoppingToken
                );

                _logger.LogInformation("✅ Escuchando eventos PagoAprobado y PagoRechazado...");

                // Consumer PagoAprobado
                var consumerAprobado = new AsyncEventingBasicConsumer(channel);
                consumerAprobado.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var mensaje = Encoding.UTF8.GetString(body);
                        var evento = JsonSerializer.Deserialize<PagoAprobadoEvent>(mensaje);

                        if (evento is not null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<OrdenesDbContext>();
                            var orden = await db.Ordenes.FirstOrDefaultAsync(o => o.Id == evento.OrdenId);

                            if (orden is not null)
                            {
                                orden.Estado = "confirmada";
                                orden.ActualizadoEn = DateTime.UtcNow;
                                await db.SaveChangesAsync();
                                _logger.LogInformation("✅ Orden {OrdenId} confirmada", evento.OrdenId);
                            }
                        }

                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando PagoAprobado");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };

                // Consumer PagoRechazado
                var consumerRechazado = new AsyncEventingBasicConsumer(channel);
                consumerRechazado.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var mensaje = Encoding.UTF8.GetString(body);
                        var evento = JsonSerializer.Deserialize<PagoRechazadoEvent>(mensaje);

                        if (evento is not null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<OrdenesDbContext>();
                            var orden = await db.Ordenes.FirstOrDefaultAsync(o => o.Id == evento.OrdenId);

                            if (orden is not null)
                            {
                                orden.Estado = "cancelada";
                                orden.ActualizadoEn = DateTime.UtcNow;
                                await db.SaveChangesAsync();
                                _logger.LogWarning("❌ Orden {OrdenId} cancelada por pago rechazado", evento.OrdenId);
                            }
                        }

                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando PagoRechazado");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: "ordenes.pago-aprobado",
                    autoAck: false,
                    consumer: consumerAprobado,
                    cancellationToken: stoppingToken
                );

                await channel.BasicConsumeAsync(
                    queue: "ordenes.pago-rechazado",
                    autoAck: false,
                    consumer: consumerRechazado,
                    cancellationToken: stoppingToken
                );

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ RabbitMQ no disponible, reintentando en 10 segundos... {Error}", ex.Message);
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}