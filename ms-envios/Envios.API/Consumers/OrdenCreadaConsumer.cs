using Envios.API.Data;
using Envios.API.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Envios.API.Consumers;

public class OrdenCreadaConsumer : BackgroundService
{
    private readonly IConfiguration      _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrdenCreadaConsumer> _logger;
    private static readonly Random _rng = new();

    public OrdenCreadaConsumer(
        IConfiguration config,
        IServiceScopeFactory scopeFactory,
        ILogger<OrdenCreadaConsumer> logger)
    {
        _config       = config;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["RabbitMQ:Host"]     ?? "rabbitmq",
                    Port     = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                    UserName = _config["RabbitMQ:User"]     ?? "admin",
                    Password = _config["RabbitMQ:Password"] ?? "Admin_12345"
                };

                using var connection = await factory.CreateConnectionAsync(stoppingToken);
                using var channel    = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange:   "tienda.ordenes",
                    type:       ExchangeType.Direct,
                    durable:    true,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue:      "envios.orden-creada",
                    durable:    true,
                    exclusive:  false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue:      "envios.orden-creada",
                    exchange:   "tienda.ordenes",
                    routingKey: "orden.creada",
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Escuchando OrdenCreada para generar envíos...");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var body   = ea.Body.ToArray();
                        var json   = Encoding.UTF8.GetString(body);
                        var evento = JsonSerializer.Deserialize<OrdenCreadaDto>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (evento is not null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                            var yaExiste = db.Envios.Any(e => e.OrderId == evento.OrdenId);
                            if (!yaExiste)
                            {
                                var numRastreo = GenerarNumRastreo();
                                var envio = new Envio
                                {
                                    OrderId          = evento.OrdenId,
                                    NumRastreo       = numRastreo,
                                    Estado           = "Preparando",
                                    DireccionEntrega = string.Empty,
                                    FechaEstimada    = DateTime.UtcNow.AddDays(3)
                                };

                                db.Envios.Add(envio);
                                await db.SaveChangesAsync();

                                db.HistorialEstados.Add(new HistorialEstado
                                {
                                    EnvioId = envio.Id,
                                    Estado  = "Preparando"
                                });
                                await db.SaveChangesAsync();

                                _logger.LogInformation(
                                    "Envío creado para orden {OrdenId} — rastreo: {NumRastreo}",
                                    evento.OrdenId, numRastreo);
                            }
                        }

                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando OrdenCreada");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue:      "envios.orden-creada",
                    autoAck:    false,
                    consumer:   consumer,
                    cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ no disponible, reintentando en 10s... {Error}", ex.Message);
                await Task.Delay(10_000, stoppingToken);
            }
        }
    }

    private static string GenerarNumRastreo()
    {
        var now    = DateTime.UtcNow;
        var serial = _rng.Next(100_000, 999_999);
        return $"TDA-{now:yyyyMM}-{serial}";
    }
}

internal class OrdenCreadaDto
{
    public int OrdenId    { get; set; }
    public int ClienteId  { get; set; }
    public decimal Total  { get; set; }
    public DateTime CreadoEn { get; set; }
}
