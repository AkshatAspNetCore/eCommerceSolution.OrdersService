namespace BusinessLogicLayer.DTO;

public record OrderUpdateRequest(Guid OrderID, Guid UserID, DateTime OrderDate, List<OrderItemUpdateRequest> OrderItems)
{
    OrderUpdateRequest() : this(default, default, default, default) { }
}
