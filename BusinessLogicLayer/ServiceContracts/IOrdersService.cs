using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace BusinessLogicLayer.ServiceContracts;

public interface IOrdersService
{
    /// <summary>
    /// Retrieves list of orders asynchronously from the orders repository.
    /// </summary>
    /// <returns>Returns list of OrderResponse objects</returns>
    Task<List<OrderResponse?>> GetOrders();

    /// <summary>
    /// Retrieves a list of orders based on a specified filter condition asynchronously using a FilterDefinition.
    /// </summary>
    /// <param name="filter">Expression that represents condition to check.</param>
    /// <returns>Returns matching orders as OrderResponse objects.</returns>
    Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Retrieves a single order based on a specified filter condition asynchronously using a FilterDefinition.
    /// </summary>
    /// <param name="filter">Expression that represents the condition to check.</param>
    /// <returns>Returns matching order object as OrderResponse; or null if not found.</returns>
    Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Adds a new order in the orders collection asynchronously by calling the AddOrder method of the orders repository and returns the added order as OrderResponse object.
    /// </summary>
    /// <param name="orderAddRequest">Order to insert.</param>
    /// <returns>Returns OrderResponse object that contains the order details like OrderID, UserID, OrderItems, TotalBill, OrderDate after inserting; or returns null if insertion is unsuccessful.</returns>
    Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest);

    /// <summary>
    /// Uddates an existing order in the orders collection asynchronously by calling the UpdateOrder method of the orders repository and returns the updated order as OrderResponse object.
    /// </summary>
    /// <param name="orderUpdateRequest">Order data to update..</param>
    /// <returns>Returns updated order object after successful updation; otherwise null.</returns>
    Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest);

    /// <summary>
    /// Deletes an existing order from the orders collection asynchronously by calling the DeleteOrder method of the orders repository and returns true if deletion is successful otherwise false.
    /// </summary>
    /// <param name="orderID">OrderID to search and delete.</param>
    /// <returns>Returns true if deletion is successful; otherwise false.</returns>
    Task<bool> DeleteOrder(Guid orderID);
}
