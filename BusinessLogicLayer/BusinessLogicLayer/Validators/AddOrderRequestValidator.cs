using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class AddOrderRequestValidator: AbstractValidator<OrderAddRequest>
{
    public AddOrderRequestValidator()
    {
        //UserID
        RuleFor(x => x.UserID).NotEmpty().WithErrorCode("UserID is required.");

        //OrderDate
        RuleFor(x => x.OrderDate).NotEmpty().WithErrorCode("OrderDate is required.");

        //OrderItems
        RuleFor(x => x.OrderItems).NotEmpty().WithErrorCode("OrderItems are required.")
            .Must(items => items != null && items.Count > 0).WithMessage("At least one order item is required.");
    }
}
