using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ECommerceSolution.OrderService.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        //GET api/Orders
        [HttpGet]
        public async Task<ActionResult<List<OrderResponse?>>> Get()
        {
            List<OrderResponse?> orders = await _ordersService.GetOrders();
            return orders;
        }

        //GET api/Orders/search/orderid/{orderID}
        [HttpGet("search/orderid/{orderID}")]
        public async Task<OrderResponse?> GetOrderByOrderID(Guid orderID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);

            OrderResponse? order = await _ordersService.GetOrderByCondition(filter);
            return order;
        }

        //GET api/Orders/search/productid/{productID}
        [HttpGet("search/productid/{productID}")]
        public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductID(Guid productID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems, 
                Builders<OrderItem>.Filter.Eq(tempProduct => tempProduct.ProductID, productID));

            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;
        }

        //GET api/Orders/search/orderDate/{orderDate}
        [HttpGet("search/orderDate/{orderDate}")]
        public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDate(DateTime orderDate)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyy-MM-dd"),
                orderDate.ToString("yyy-MM-dd"));

            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;
        }

        //POST api/Orders
        [HttpPost]
        public async Task<IActionResult> Post(OrderAddRequest orderAddRequest) 
        {
            if (orderAddRequest == null)
            {
                return BadRequest("Invalid order data.");
            }

            OrderResponse? orderResponse = await _ordersService.AddOrder(orderAddRequest);

            if (orderResponse == null) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to add order.");
            }
            
            return Created($"api/orders/search/orderid/{orderResponse?.OrderID}", orderResponse);
        }

        //PUT api/Orders/{orderID}
        [HttpPut("{orderID}")]
        public async Task<IActionResult> Put(Guid orderID, OrderUpdateRequest orderUpdateRequest)
        {
            if (orderUpdateRequest == null)
            {
                return BadRequest("Invalid order data.");
            }

            if (orderID != orderUpdateRequest.OrderID) 
            {
                return BadRequest("Order ID in the URL does not match Order ID in the request body.");
            }

            OrderResponse? updateOrderResponse = await _ordersService.UpdateOrder(orderUpdateRequest);

            if (updateOrderResponse == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update order.");
            }

            return Ok(updateOrderResponse);
        }


        //DELETE api/Orders/{orderID}
        [HttpDelete("{orderID}")]
        public async Task<IActionResult> Delete(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                return BadRequest("Invalid order ID.");
            }

            bool isDeleted = await _ordersService.DeleteOrder(orderID);

            if (!isDeleted)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete order.");
            }

            return Ok(isDeleted);
        }

        //GET api/Orders/search/userid/{UserID}
        [HttpGet("search/userid/{UserID}")]
        public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserID(Guid UserID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.UserID, UserID);

            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;
        }
    }
}
