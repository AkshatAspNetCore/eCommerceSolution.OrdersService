namespace BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductNameUpdateConsumer
    {
        Task Consume();
        void Dispose();
    }
}