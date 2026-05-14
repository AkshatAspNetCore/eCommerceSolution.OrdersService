using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionHostedService : IHostedService
{
    private readonly IRabbitMQProductDeletionConsumer _productDeletionConsumer;

    public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer consumer)
    {
        _productDeletionConsumer = consumer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _productDeletionConsumer.Consume();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productDeletionConsumer.Dispose();
        return Task.CompletedTask;
    }
}
