using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MsPagos.Services;

public class RabbitMQPublisher
{
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(IConfiguration config, ILogger<RabbitMQPublisher> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task PublicarAsync<T>(string exchange, string routingKey, T evento)
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

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false
            );

            var mensaje = JsonSerializer.Serialize(evento);
            var body = Encoding.UTF8.GetBytes(mensaje);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Evento publicado: {Exchange} - {RoutingKey}", exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando evento en RabbitMQ");
        }
    }
}