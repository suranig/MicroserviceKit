using FluentValidation;

namespace EventStoreService.Application.Event.Commands.DeleteEvent;

public class DeleteEventCommandValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventCommandValidator()
    {
        // Add validation rules as needed
    }
}