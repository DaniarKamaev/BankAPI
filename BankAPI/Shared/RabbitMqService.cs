using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BankAPI.Shared;

public class RabbitMqService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IConfiguration config, ILogger<RabbitMqService> logger)
    {
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"] ?? "localhost",
                UserName = config["RabbitMQ:Username"] ?? "guest",
                Password = config["RabbitMQ:Password"] ?? "guest",
                Port = config.GetValue("RabbitMQ:Port", 5672),
                DispatchConsumersAsync = true
            };

            // Явное создание соединения через фабрику
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            InitializeRabbitMq();

            _logger.LogInformation("RabbitMQ connection established");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    private void InitializeRabbitMq()
    {
        try
        {
            // Объявление exchange
            _channel.ExchangeDeclare(
                exchange: "account.events",
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null);

            // Объявление очередей
            DeclareQueue("account.crm", "account.*");
            DeclareQueue("account.notifications", "money.*");
            DeclareQueue("account.antifraud", "client.*");
            DeclareQueue("account.audit", "#");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ topology");
            throw;
        }
    }

    private void DeclareQueue(string queueName, string routingKey)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: queueName,
            exchange: "account.events",
            routingKey: routingKey,
            arguments: null);
    }

    public void Publish<T>(T message, string routingKey, Guid correlationId)
    {
        try
        {
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            props.CorrelationId = correlationId.ToString();
            props.Headers = new Dictionary<string, object>
            {
                { "X-Correlation-Id", correlationId.ToString() },
                { "X-Causation-Id", Guid.NewGuid().ToString() }
            };

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: "account.events",
                routingKey: routingKey,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Published message to {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to {RoutingKey}", routingKey);
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _logger.LogInformation("RabbitMQ connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ resources");
        }
        GC.SuppressFinalize(this);
    }
}