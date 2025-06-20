using FluentValidation;

namespace ReadModelService.Application.Page.Commands.UpdatePage;

public class UpdatePageCommandValidator : AbstractValidator<UpdatePageCommand>
{
    public UpdatePageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}