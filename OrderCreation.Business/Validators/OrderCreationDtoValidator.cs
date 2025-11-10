using FluentValidation;
using OrderCreation.Business.Dto;

namespace OrderCreation.Business.Validators
{
    public class OrderCreationDtoValidator : AbstractValidator<OrderCreationDto>
    {
        public OrderCreationDtoValidator()
        {
            RuleFor(o => o.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(o => o.Product)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

            RuleFor(o => o.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(o => o.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }
}
