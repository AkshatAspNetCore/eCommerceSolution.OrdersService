using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.RepositoryContracts;

public interface IOrdersRepository
{
    /// <summary>
    /// Retrieves all orders asynchronously from the database.
    /// </summary>
    /// <returns>Returns all orders from orders collection.</returns>
    Task<IEnumerable<Order>> GetOrders();

    /// <summary>
    /// Retrieves orders based on a specified filter condition asynchronously.
    /// </summary>
    /// <param name="filter">The condition to filter orders.</param>
    /// <returns>Returning a collection of matching orders.</returns>
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Retrieves a single order based on a specified filter condition asynchronously using a FilterDefinition.
    /// </summary>
    /// <param name="filter">The condition to filer orders.</param>
    /// <returns>Returning matching order.</returns>
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Adds a new order in the orders collection asynchronously.
    /// </summary>
    /// <param name="order">The order to be added.</param>
    /// <returns>Returns the added order.</returns>
    Task<Order?> AddOrder(Order order);

    /// <summary>
    /// Updates an existing order in the orders collection asynchronously.
    /// </summary>
    /// <param name="order">The order to be updated.</param>
    /// <returns>Returns an updated order object or returns null if no order found.</returns>
    Task<Order?> UpdateOrder(Order order);

    /// <summary>
    /// Deletes an existing order from the orders collection asynchronously.
    /// </summary>
    /// <param name="orderID">The ID of the order to be deleted.</param>
    /// <returns>Returns true if deletion is successful otherwise false.</returns>
    Task<bool> DeleteOrder(Guid orderID);
}
