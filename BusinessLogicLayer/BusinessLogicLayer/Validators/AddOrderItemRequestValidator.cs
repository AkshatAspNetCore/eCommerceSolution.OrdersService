using BusinessLogicLayer.DTO;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

public class AddOrderItemRequestValidator: AbstractValidator<OrderItemAddRequest>
{
    public AddOrderItemRequestValidator()
    {
        //ProductID
        RuleFor(x => x.ProductID).NotEmpty().WithErrorCode("ProductID is required.");

        //UnitPrice
        RuleFor(x => x.UnitPrice).NotEmpty().WithErrorCode("Unit Price is required.")
            .GreaterThan(0).WithMessage("Unit Price must be greater than 0.");

        //Quantity
        RuleFor(x => x.Quantity).NotEmpty().WithErrorCode("Quantity is required.")
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}
