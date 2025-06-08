using FluentValidation;

namespace ECommerce.OrderService.Application.Customer.Commands.UpdateEmailCustomer;

public class UpdateEmailCustomerCommandValidator : AbstractValidator<UpdateEmailCustomerCommand>
{
    public UpdateEmailCustomerCommandValidator()
    {
        // Add validation rules as needed
    }
}