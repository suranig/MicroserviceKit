using FluentValidation;

namespace Company.TestService.Application.Item.Commands.CreateItem;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
    }
}