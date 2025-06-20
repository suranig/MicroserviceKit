using FluentValidation;

namespace ReadModelService.Application.Page.Commands.CreatePage;

public class CreatePageCommandValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}