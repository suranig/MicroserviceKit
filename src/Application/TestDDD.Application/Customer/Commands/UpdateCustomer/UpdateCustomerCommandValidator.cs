using FluentValidation;

namespace ECommerce.OrderService.Application.Customer.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}