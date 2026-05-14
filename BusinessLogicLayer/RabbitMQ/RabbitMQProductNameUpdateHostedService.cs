using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateHostedService : IHostedService
{
    private readonly IRabbitMQProductNameUpdateConsumer _productNameUpdateConsumer;

    public RabbitMQProductNameUpdateHostedService(IRabbitMQProductNameUpdateConsumer consumer)
    {
        _productNameUpdateConsumer = consumer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _productNameUpdateConsumer.Consume();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productNameUpdateConsumer.Dispose();
        return Task.CompletedTask;
    }
}
