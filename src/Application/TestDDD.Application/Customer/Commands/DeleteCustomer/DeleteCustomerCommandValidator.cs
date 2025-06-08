using FluentValidation;

namespace ECommerce.OrderService.Application.Customer.Commands.DeleteCustomer;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        // Add validation rules as needed
    }
}