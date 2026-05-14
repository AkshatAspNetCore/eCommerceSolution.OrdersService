using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services;

public class OrdersService : IOrdersService
{
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IOrdersRepository _ordersRepository;
    private UsersMicroserviceClient _usersMicroserviceClient;
    private ProductsMicroserviceClient _productsMicroserviceClient;

    public OrdersService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _mapper = mapper;
        _ordersRepository = ordersRepository;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
    }

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        //Check if the orderAddRequest is valid
        if (orderAddRequest == null) 
        {
            throw new ArgumentNullException(nameof(orderAddRequest), "OrderAddRequest cannot be null.");
        }

        List<ProductDTO> products = new List<ProductDTO>();

        //Validate the orderAddRequest using FluentValidation
        ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidationResult.IsValid)
        {
            string errorMessages = string.Join(Environment.NewLine, orderAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errorMessages);
        }

        //Validate each OrderItemAddRequest in the OrderAddRequest
        foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errorMessages = string.Join(Environment.NewLine, orderItemAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errorMessages);
            }

            //TO DO: Add logic for checking if ProductID exists in Products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemAddRequest.ProductID);

            if (product == null) throw new ArgumentException($"Product with ProductID {orderItemAddRequest.ProductID} does not exist in Products microservice.");

            products.Add(product);
        }

        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID); 

        if(user == null) throw new ArgumentException($"User with UserID {orderAddRequest.UserID} does not exist in Users microservice.");

        //Map OrderAddRequest to Order entity
        Order orderInput = _mapper.Map<Order>(orderAddRequest); //Map OrderAddRequest to Order entity (it invokes OrderAddRequestToOrderMappingProfile class)

        //Generate TotalBill
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice; 
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

        //Invoke Repository method to add order and return the added order as OrderResponse object
        Order? addedOrder = await _ordersRepository.AddOrder(orderInput);

        if (addedOrder == null)
        {
            return null;
        }

        OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder); //Map added Order entity to OrderResponse object (it invokes OrderToOrderResponseMappingProfile class)

        //TO DO: Load Product Name and Category in OrderItem
        if (addedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in addedOrderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.FirstOrDefault(temp => temp.ProductID == orderItemResponse.ProductID);
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse); //Map ProductDTO to OrderItemResponse object (it invokes ProductDTOToOrderItemResponseMappingProfile class)
                }
            }
        }

        //TO DO: Load Username and Email in OrderResponse
        if (user != null && addedOrderResponse != null)
        {
            _mapper.Map(user, addedOrderResponse); //Map UserDTO to AddedOrderResponse object (it invokes UserDTOToOrderResponseMappingProfile class)
        }
        return addedOrderResponse;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter); 

        if(existingOrder == null)
        {
            return false;
        }

        bool isOrderDeleted = await _ordersRepository.DeleteOrder(orderID); 
        return isOrderDeleted;
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);

        if(order == null) return null;

        OrderResponse? orderResponse = _mapper.Map<OrderResponse>(order); //Map Order entity to OrderResponse object (it invokes OrderToOrderResponseMappingProfile class)

        //TO DO: Load Product Name and Category in OrderItem
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse); //Map ProductDTO to OrderItemResponse object (it invokes ProductDTOToOrderItemResponseMappingProfile class)
                }
            }
        }

        //TO DO: Load Username and Email in OrderResponse
        if (orderResponse != null)
        {
            UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO != null)
            {
                _mapper.Map(userDTO, orderResponse); //Map UserDTO to OrderResponse object (it invokes UserDTOToOrderResponseMappingProfile class)
            }
        }

        return orderResponse;
    }

    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrders();
        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse?>>(orders); //Map Order entity to OrderResponse object (it invokes OrderToOrderResponseMappingProfile class)

        //TO DO: Load Product Name and Category in OrderItem
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse != null)
            {
                foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
                {
                    ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                    if (productDTO != null)
                    {
                        _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse); //Map ProductDTO to OrderItemResponse object (it invokes ProductDTOToOrderItemResponseMappingProfile class)
                    }
                }

                //TO DO: Load Username and Email in OrderResponse
                UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
                if (userDTO != null)
                {
                    _mapper.Map(userDTO, orderResponse); //Map UserDTO to OrderResponse object (it invokes UserDTOToOrderResponseMappingProfile class)
                }
            }
        }

        return orderResponses.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);

        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse?>>(orders); //Map Order entity to OrderResponse object (it invokes OrderToOrderResponseMappingProfile class)

        //TO DO: Load Product Name and Category in OrderItem
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse != null)
            {
                foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
                {
                    ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                    if (productDTO != null)
                    {
                        _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse); //Map ProductDTO to OrderItemResponse object (it invokes ProductDTOToOrderItemResponseMappingProfile class)
                    }
                }

                //TO DO: Load Username and Email in OrderResponse
                UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
                if (userDTO != null)
                {
                    _mapper.Map(userDTO, orderResponse); //Map UserDTO to OrderResponse object (it invokes UserDTOToOrderResponseMappingProfile class)
                }
            }
        }
        return orderResponses.ToList();
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        //Check if the orderUpdateRequest is valid
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest), "orderUpdateRequest cannot be null.");
        }

        List<ProductDTO> products = new List<ProductDTO>();

        //Validate the orderAddRequest using FluentValidation
        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errorMessages = string.Join(Environment.NewLine, orderUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errorMessages);
        }

        //Validate each OrderItemAddRequest in the OrderAddRequest
        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);
            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errorMessages = string.Join(Environment.NewLine, orderItemUpdateRequestValidationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errorMessages);
            }

            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemUpdateRequest.ProductID);

            if (product == null) throw new ArgumentException($"Product with ProductID {orderItemUpdateRequest.ProductID} does not exist in Products microservice.");

            products.Add(product);
        }

        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);

        if (user == null) throw new ArgumentException($"User with UserID {orderUpdateRequest.UserID} does not exist in Users microservice.");

        //Map OrderUpdateRequest to Order entity
        Order orderInput = _mapper.Map<Order>(orderUpdateRequest); //Map OrderUpdateRequest to Order entity (it invokes OrderUpdateRequestToOrderMappingProfile class)

        //Generate TotalBill
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

        //Invoke Repository method to update order and return the updated order as OrderResponse object
        Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);

        if (updatedOrder == null)
        {
            return null;
        }

        OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder); //Map updated Order entity to OrderResponse object (it invokes OrderToOrderResponseMappingProfile class)

        if (updatedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.FirstOrDefault(temp => temp.ProductID == orderItemResponse.ProductID);
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse); //Map ProductDTO to OrderItemResponse object (it invokes ProductDTOToOrderItemResponseMappingProfile class)
                }
            }
        }

        //TO DO: Load Username and Email in OrderResponse
        if (user != null && updatedOrderResponse != null)
        {
            _mapper.Map(user, updatedOrderResponse); //Map UserDTO to UpdatedOrderResponse object (it invokes UserDTOToOrderResponseMappingProfile class)
        }

        return updatedOrderResponse;
    }


}
