using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using MsPagos.Events;
using MsPagos.Services;

namespace MsPagos.Handlers;

public class OrdenCreadaConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrdenCreadaConsumer> _logger;

    public OrdenCreadaConsumer(IConfiguration config, IServiceScopeFactory scopeFactory, ILogger<OrdenCreadaConsumer> logger)
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

            await channel.ExchangeDeclareAsync(
                exchange: "tienda.ordenes",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            await channel.QueueDeclareAsync(
                queue: "pagos.orden-creada",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            await channel.QueueBindAsync(
                queue: "pagos.orden-creada",
                exchange: "tienda.ordenes",
                routingKey: "orden.creada",
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("✅ Escuchando eventos OrdenCreada...");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var mensaje = Encoding.UTF8.GetString(body);
                    var evento = JsonSerializer.Deserialize<OrdenCreadaEvent>(mensaje);

                    if (evento is not null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var pagoService = scope.ServiceProvider.GetRequiredService<PagoService>();
                        await pagoService.ProcesarOrdenCreadaAsync(evento);
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
                queue: "pagos.orden-creada",
                autoAck: false,
                consumer: consumer,
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
}}