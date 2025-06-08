using FluentValidation;

namespace ECommerce.OrderService.Application.Customer.Commands.UpdateNameCustomer;

public class UpdateNameCustomerCommandValidator : AbstractValidator<UpdateNameCustomerCommand>
{
    public UpdateNameCustomerCommandValidator()
    {
        // Add validation rules as needed
    }
}