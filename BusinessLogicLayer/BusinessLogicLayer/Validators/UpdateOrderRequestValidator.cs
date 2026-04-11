using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class UpdateOrderRequestValidator: AbstractValidator<OrderUpdateRequest>
{
    public UpdateOrderRequestValidator()
    {
        //OrderID
        RuleFor(x => x.OrderID).NotEmpty().WithErrorCode("OrderID is required.");

        //UserID
        RuleFor(x => x.UserID).NotEmpty().WithErrorCode("UserID is required.");

        //OrderDate
        RuleFor(x => x.OrderDate).NotEmpty().WithErrorCode("Order Date is required.");

        //OrderItems
        RuleFor(x => x.OrderItems).NotEmpty().WithErrorCode("Order Items are required.")
            .Must(items => items != null && items.Count > 0).WithMessage("At least one order item is required.");
    }
}
