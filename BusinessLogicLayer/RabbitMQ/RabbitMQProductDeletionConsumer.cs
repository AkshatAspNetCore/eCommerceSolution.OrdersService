using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
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
    private readonly IDistributedCache _cache;

    public RabbitMQProductDeletionConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeletionConsumer> logger, IDistributedCache cache)
    {
        _logger = logger;
        _configuration = configuration;
        _cache = cache;

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

        //string routingKey = "product.#";
        var headers = new Dictionary<string, object?>()
        {
            {"x-match","all" },
            {"event","product.delete" },
            {"RowCount",1 }
        };

        string queueName = "orders.product.delete.queue";

        // Create exchange if it doesn't exist
        string exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;
        await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        // Create message queue if it doesn't exist
        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Bind the queue to the exchange with the routing key
        await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            await HandleProductDeletionMessage(message);
            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }

    private async Task HandleProductDeletionMessage(string message) 
    {
        ProductDTO? product = JsonSerializer.Deserialize<ProductDTO>(message);
        if (product is not null)
        {
            string cacheKey = $"product:{product.ProductID}";
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation($"Product deleted:{product.ProductID}, Product name:{product.ProductName}");
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}