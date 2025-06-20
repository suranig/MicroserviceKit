using FluentValidation;

namespace TestService.Application.Order.Commands.DeleteOrder;

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        // Add validation rules as needed
    }
}