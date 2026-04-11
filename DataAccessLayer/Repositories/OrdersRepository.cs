using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _ordersCollection;
    private readonly string _collectionName = "orders";

    public OrdersRepository(IMongoDatabase mongoDatabase)
    {
        _ordersCollection = mongoDatabase.GetCollection<Order>(_collectionName);
    }

    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderID = Guid.NewGuid();
        order._Id = order.OrderID;

        foreach (OrderItem orderItem in order.OrderItems)
        {
            orderItem._Id = Guid.NewGuid();
        }

        await _ordersCollection.InsertOneAsync(order);
        return order;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp =>
        temp.OrderID, orderID);

        Order? existingOrder = (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
        if(existingOrder == null)         {
            return false;
        }

        DeleteResult deletedOrder = await _ordersCollection.DeleteOneAsync(filter);
        return deletedOrder.DeletedCount > 0;

    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        return (await _ordersCollection.FindAsync(filter)).FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetOrders()
    {
        return (await _ordersCollection.FindAsync(Builders<Order>.Filter.Empty)).ToList();
    }

    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return (await _ordersCollection.FindAsync(filter)).ToList();
    }

    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp =>
        temp.OrderID, order.OrderID);

        Order? existingOrder = (await _ordersCollection.FindAsync(filter)).FirstOrDefault();

        if(existingOrder == null)         {
            return null;
        }

        order._Id = existingOrder._Id; 

        ReplaceOneResult replacedOrder =  await _ordersCollection.ReplaceOneAsync(filter, order);
        return order;
    }
}
