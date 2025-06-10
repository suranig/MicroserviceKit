using FluentValidation;

namespace Company.TestService.Application.Item.Commands.MarkCompleteItem;

public class MarkCompleteItemCommandValidator : AbstractValidator<MarkCompleteItemCommand>
{
    public MarkCompleteItemCommandValidator()
    {
        // Add validation rules as needed
    }
}