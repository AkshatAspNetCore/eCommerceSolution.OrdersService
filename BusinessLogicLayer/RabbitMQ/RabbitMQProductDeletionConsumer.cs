using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionConsumer : IDisposable, IRabbitMQProductDeletionConsumer
{
    private readonly IConfiguration _configuration;
    private readonly ConnectionFactory _connectionFactory;
    private IChannel? _channel;
    private IConnection? _connection;
    private readonly ILogger<RabbitMQProductDeletionConsumer> _logger;

    public RabbitMQProductDeletionConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeletionConsumer> logger)
    {
        _logger = logger;
        _configuration = configuration;

        string hostName = _configuration["RABBITMQ_HOST"]!;
        string userName = _configuration["RABBITMQ_USER"]!;
        string password = _configuration["RABBITMQ_PASSWORD"]!;
        string port = _configuration["RABBITMQ_PORT"]!;

        _connectionFactory = new()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };
    }

    public async Task InitializeConnection()
    {
        _connection = await _connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task Consume()
    {
        if (_channel == null) await InitializeConnection();

        string routingKey = "product.delete";
        string queueName = "orders.product.delete.queue";

        // Create exchange if it doesn't exist
        string exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;
        await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        // Create message queue if it doesn't exist
        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Bind the queue to the exchange with the routing key
        await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                ProductDeletionMessage? productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);

                if (productDeletionMessage is not null)
                    _logger.LogInformation($"Product is deleted:{productDeletionMessage.ProductID}, Product name:{productDeletionMessage.ProductName}");
            }

            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}