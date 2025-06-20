using FluentValidation;

namespace TestService.Application.Order.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        // Add validation rules as needed
    }
}