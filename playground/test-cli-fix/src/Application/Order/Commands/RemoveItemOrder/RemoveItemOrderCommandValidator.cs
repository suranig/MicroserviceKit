using FluentValidation;

namespace TestService.Application.Order.Commands.RemoveItemOrder;

public class RemoveItemOrderCommandValidator : AbstractValidator<RemoveItemOrderCommand>
{
    public RemoveItemOrderCommandValidator()
    {
        // Add validation rules as needed
    }
}