using FluentValidation;

namespace ECommerce.OrderService.Application.Order.Commands.RemoveItemOrder;

public class RemoveItemOrderCommandValidator : AbstractValidator<RemoveItemOrderCommand>
{
    public RemoveItemOrderCommandValidator()
    {
        // Add validation rules as needed
    }
}