namespace BusinessLogicLayer.DTO;

public record OrderResponse(Guid OrderID, Guid UserID, string? Username, string? Email, decimal TotalBill, DateTime OrderDate, List<OrderItemResponse> OrderItems)
{
    OrderResponse() : this(default, default, default, default, default, default, default) { }
}
