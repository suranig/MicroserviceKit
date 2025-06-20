using FluentValidation;

namespace EventStoreService.Application.Event.Commands.UpdateEvent;

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(x => x.Description).MaximumLength(100).WithMessage("Description cannot exceed 100 characters");
    }
}