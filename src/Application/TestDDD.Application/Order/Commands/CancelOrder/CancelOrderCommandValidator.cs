using FluentValidation;

namespace ECommerce.OrderService.Application.Order.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        // Add validation rules as needed
    }
}