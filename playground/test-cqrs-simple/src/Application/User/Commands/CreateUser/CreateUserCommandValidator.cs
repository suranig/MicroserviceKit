using FluentValidation;

namespace SimpleService.Application.User.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(x => x.Description).MaximumLength(100).WithMessage("Description cannot exceed 100 characters");
    }
}