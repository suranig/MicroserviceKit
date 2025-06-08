using FluentValidation;

namespace ECommerce.OrderService.Application.Order.Commands.ConfirmOrder;

public class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        // Add validation rules as needed
    }
}