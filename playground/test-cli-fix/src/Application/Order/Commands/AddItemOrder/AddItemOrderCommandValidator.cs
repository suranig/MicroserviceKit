using FluentValidation;

namespace TestService.Application.Order.Commands.AddItemOrder;

public class AddItemOrderCommandValidator : AbstractValidator<AddItemOrderCommand>
{
    public AddItemOrderCommandValidator()
    {
        // Add validation rules as needed
    }
}