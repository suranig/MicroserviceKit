using FluentValidation;

namespace Company.TestService.Application.Item.Commands.DeleteItem;

public class DeleteItemCommandValidator : AbstractValidator<DeleteItemCommand>
{
    public DeleteItemCommandValidator()
    {
        // Add validation rules as needed
    }
}